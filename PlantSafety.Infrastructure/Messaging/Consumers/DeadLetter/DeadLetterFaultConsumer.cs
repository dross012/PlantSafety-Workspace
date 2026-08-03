// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Messaging.Consumers.DeadLetter
//  Layer:          Infrastructure
//  File:           DeadLetterFaultConsumer.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Eventing & Messaging Agent, Phase 6)
//  SchemaVersion:  2026.1
//  Description:    Generic MassTransit fault sink: consumes MassTransit's built-in
//                  Fault{TMessage} event (published automatically once
//                  cfg.UseMessageRetry(...) exhausts every configured retry attempt for a
//                  faulted consumer of TMessage) and records it in
//                  [System].[DeadLetterEvents]. Registered ONCE, as an open generic consumer
//                  (x.AddConsumer(typeof(DeadLetterFaultConsumer<>))), which is MassTransit's
//                  documented pattern for a single, message-type-agnostic global fault handler --
//                  no per-consumer/per-event-type registration is required. Prior to this pass,
//                  AddMassTransit's configuration (InfrastructureServiceRegistration.cs) had no
//                  UseMessageRetry(...) middleware and no fault-handling consumer of any kind, so
//                  every poisoned/permanently-failing message silently vanished into RabbitMQ's
//                  transport-level "_error" queue with no queryable application-level trail.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Messaging.Consumers.DeadLetter;

/// <summary>
/// Consumes MassTransit's <see cref="Fault{T}"/> for every faulted message type and persists a
/// diagnostic row to <c>[System].[DeadLetterEvents]</c>.
/// </summary>
/// <typeparam name="TMessage">The original (faulted) message type.</typeparam>
public sealed class DeadLetterFaultConsumer<TMessage>(
    ISqlConnectionFactory connectionFactory,
    ILogger<DeadLetterFaultConsumer<TMessage>> logger)
    : IConsumer<Fault<TMessage>>
    where TMessage : class
{
    /// <inheritdoc/>
    public async Task Consume(ConsumeContext<Fault<TMessage>> context)
    {
        var fault = context.Message;
        var faultedMessage = fault.Message;

        var (eventType, schemaVersion) = ResolveDiscriminator(faultedMessage);
        var lastException = fault.Exceptions is { Length: > 0 } ? fault.Exceptions[^1] : null;

        var deadLetterId = Uuid7.New();
        var messageId = context.Headers.Get<string>(OutboxHeaderNames.OutboxMessageId) is { } rawMessageId
            && Guid.TryParse(rawMessageId, out var parsedMessageId)
                ? parsedMessageId
                : (Guid?)null;

        var tenantId = context.Headers.Get<string>(OutboxHeaderNames.TenantId);
        var actorId = context.Headers.Get<string>(OutboxHeaderNames.ActorId);
        var correlationId = context.GetEnvelopeCorrelationId();

        // Best-effort receive-endpoint/consumer identification: this codebase's MassTransit
        // consumers follow a strict 1:1 consumer-per-message-type convention (see
        // IdempotentConsumerBase<TMessage> / CrossModuleIdempotentConsumerBase<TMessage>), and
        // MassTransit's default endpoint-naming convention names each receive endpoint after its
        // consumer type -- so the faulting receive endpoint's input address is a reliable proxy
        // for "which consumer faulted" without requiring MassTransit to expose the consumer CLR
        // type directly on Fault<T> (it does not).
        var consumerName = context.ReceiveContext?.InputAddress?.AbsolutePath?.TrimStart('/')
            ?? typeof(TMessage).Name;

        try
        {
            using var conn = connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("DeadLetterId", deadLetterId);
            parameters.Add("MessageId", messageId);
            parameters.Add("EventType", eventType);
            parameters.Add("SchemaVersion", schemaVersion);
            parameters.Add("EnvelopeTenantId", tenantId);
            parameters.Add("Payload", SerializePayload(faultedMessage));
            parameters.Add("FaultReason", BuildFaultReason(lastException));
            parameters.Add("ExceptionDetail", BuildExceptionDetail(fault));
            parameters.Add("RetryCount", fault.Exceptions?.Length ?? 0);
            parameters.Add("FaultedAt", fault.Timestamp);
            parameters.Add("CorrelationId", correlationId);
            // MassTransit's own InitiatorId identifies the message that initiated this conversation
            // (i.e. the immediate parent operation) -- the closest transport-level analogue to this
            // codebase's CausationId concept (§2.17 Canonical Correlation & Causality Lineage Rule).
            // Falls back to MassTransit's ConversationId when InitiatorId is absent.
            parameters.Add("CausationId", context.InitiatorId ?? context.ConversationId);
            parameters.Add("ActorId", actorId);
            parameters.Add("ConsumerName", consumerName);

            await conn.ExecuteAsync(
                sql: "[System].[usp_DeadLetterEvents_Insert]",
                param: parameters,
                commandType: CommandType.StoredProcedure);

            logger.LogError(
                "DeadLetterFaultConsumer: {EventType} DeadLetterId={DeadLetterId} CorrelationId={CorrelationId} " +
                "moved to dead-letter after {RetryCount} attempt(s): {FaultReason}",
                eventType, deadLetterId, correlationId, fault.Exceptions?.Length ?? 0, BuildFaultReason(lastException));
        }
        catch (Exception ex)
        {
            // The dead-letter sink itself must never throw back into the fault pipeline -- doing so
            // would re-fault the Fault<T> consumption and could loop. Log at Critical-equivalent
            // (Error) severity so the underlying persistence failure is still visible, but swallow
            // rather than propagate.
            logger.LogError(ex,
                "DeadLetterFaultConsumer: failed to persist dead-letter row for {EventType} (original fault: {FaultReason}).",
                eventType, BuildFaultReason(lastException));
        }
    }

    private static (string EventType, string SchemaVersion) ResolveDiscriminator(TMessage message)
    {
        // Domain/integration events in this codebase implement IDomainEvent (EventType/
        // SchemaVersion) -- mirror OutboxProcessor/OutboxEventTypeRegistry's convention so the
        // dead-letter row's EventType matches the same discriminator used everywhere else in the
        // eventing pipeline. Falls back to the CLR type name for non-domain-event message types
        // (e.g. the raw OutboxEnvelope fallback path).
        if (message is IDomainEvent domainEvent)
        {
            return (domainEvent.EventType, domainEvent.SchemaVersion);
        }

        return (typeof(TMessage).Name, InfrastructureConstants.EventSchemaVersion);
    }

    private static string? SerializePayload(TMessage message)
    {
        try
        {
            return JsonSerializer.Serialize(message, typeof(TMessage), JsonSerializerConfiguration.Default);
        }
        catch
        {
            // Some Smart Enum value objects cannot round-trip through System.Text.Json without a
            // bespoke converter (see OutboxProcessor.PublishAsync's identical fallback reasoning).
            // Losing the payload is preferable to losing the entire dead-letter row.
            return null;
        }
    }

    private static string BuildFaultReason(ExceptionInfo? exception) =>
        exception is null
            ? "Unknown fault (no exception detail available)."
            : $"{exception.ExceptionType}: {Truncate(exception.Message ?? string.Empty, 380)}";

    private static string BuildExceptionDetail(Fault<TMessage> fault)
    {
        if (fault.Exceptions is not { Length: > 0 })
        {
            return "No exception detail captured.";
        }

        var attempts = fault.Exceptions.Select((e, i) =>
            $"Attempt {i + 1}: {e.ExceptionType}: {e.Message}{(string.IsNullOrEmpty(e.StackTrace) ? string.Empty : $"\n{e.StackTrace}")}");

        return string.Join("\n---\n", attempts);
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength] + "…";
}
