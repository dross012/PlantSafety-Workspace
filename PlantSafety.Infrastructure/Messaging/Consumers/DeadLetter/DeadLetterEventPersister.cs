// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Messaging.Consumers.DeadLetter
//  Layer:          Infrastructure
//  File:           DeadLetterEventPersister.cs
//  Created:        2026-08-02
//  Updated:        2026-08-03
//  Author:         PlantSafety Platform Team (Eventing & Messaging Agent, Phase 6)
//  SchemaVersion:  2026.1
//  Description:    Reusable, non-generic-registration persistence logic for the dead-letter sink.
//                  Extracted from the former open-generic DeadLetterFaultConsumer{TMessage}
//                  (IConsumer{Fault{TMessage}}) so it can be invoked from
//                  DeadLetterFaultObserver.ConsumeFault{T} (an IConsumeObserver callback) instead.
//                  The open-generic AddConsumer(typeof(DeadLetterFaultConsumer{})) registration
//                  produced an internal MassTransit RegisterConsumer{} marker type with an unbound
//                  generic parameter, which PlantSafety.Api's strict
//                  UseDefaultServiceProvider(ValidateOnBuild = true) eagerly tried to
//                  Activator.CreateInstance at startup and crashed with
//                  "Cannot create an instance of ... because Type.ContainsGenericParameters is
//                  true." Registered as a normal singleton -- no open generics anywhere in the DI
//                  container. Preserves all prior persisted-row semantics (EventType/SchemaVersion
//                  via IDomainEvent, correlation/causation/actor/tenant recovery via
//                  OutboxHeaderNames, retry count, payload, fault reason/detail) with one
//                  unavoidable shape change: MassTransit's IConsumeObserver.ConsumeFault{T} hands
//                  back the single, final Exception (not Fault{T}.Exceptions, the full per-attempt
//                  array), and the per-attempt retry count is recovered via
//                  RetryContextExtensions.GetRetryCount(context) instead of
//                  Fault{T}.Exceptions.Length. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Messaging.Consumers.DeadLetter;

/// <summary>
/// Persists a diagnostic dead-letter row to <c>[System].[DeadLetterEvents]</c> for a message that
/// permanently failed (i.e. after <c>UseMessageRetry(...)</c> exhausted its configured budget).
/// </summary>
public interface IDeadLetterEventPersister
{
    /// <summary>Persists a dead-letter row for the given faulted message and terminal exception.</summary>
    Task PersistAsync<TMessage>(ConsumeContext<TMessage> context, Exception exception)
        where TMessage : class;
}

/// <inheritdoc cref="IDeadLetterEventPersister"/>
public sealed class DeadLetterEventPersister(
    ISqlConnectionFactory connectionFactory,
    ILogger<DeadLetterEventPersister> logger) : IDeadLetterEventPersister
{
    /// <inheritdoc/>
    public async Task PersistAsync<TMessage>(ConsumeContext<TMessage> context, Exception exception)
        where TMessage : class
    {
        var message = context.Message;
        var (eventType, schemaVersion) = ResolveDiscriminator(message);

        var deadLetterId = Uuid7.New();
        var messageId = context.Headers.Get<string>(OutboxHeaderNames.OutboxMessageId) is { } rawMessageId
            && Guid.TryParse(rawMessageId, out var parsedMessageId)
                ? parsedMessageId
                : (Guid?)null;

        var tenantId = context.Headers.Get<string>(OutboxHeaderNames.TenantId);
        var actorId = context.Headers.Get<string>(OutboxHeaderNames.ActorId);
        var correlationId = context.GetEnvelopeCorrelationId();

        // MassTransit's IConsumeObserver.ConsumeFault<T> fires once per permanently-faulted
        // message, after cfg.UseMessageRetry(...) exhausts its budget -- the exact same semantics
        // as consuming Fault<T> previously provided. RetryContextExtensions.GetRetryCount(context)
        // recovers the same "how many attempts were made" figure that Fault<T>.Exceptions.Length
        // used to.
        var retryCount = context.GetRetryCount();

        // Best-effort receive-endpoint/consumer identification: this codebase's MassTransit
        // consumers follow a strict 1:1 consumer-per-message-type convention (see
        // IdempotentConsumerBase<TMessage> / CrossModuleIdempotentConsumerBase<TMessage>), and
        // MassTransit's default endpoint-naming convention names each receive endpoint after its
        // consumer type -- so the faulting receive endpoint's input address is a reliable proxy
        // for "which consumer faulted" without requiring MassTransit to expose the consumer CLR
        // type directly on this observer callback.
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
            parameters.Add("Payload", SerializePayload(message));
            parameters.Add("FaultReason", BuildFaultReason(exception));
            parameters.Add("ExceptionDetail", BuildExceptionDetail(exception));
            parameters.Add("RetryCount", retryCount);
            parameters.Add("FaultedAt", DateTime.UtcNow);
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
                "DeadLetterFaultObserver: {EventType} DeadLetterId={DeadLetterId} CorrelationId={CorrelationId} " +
                "moved to dead-letter after {RetryCount} attempt(s): {FaultReason}",
                eventType, deadLetterId, correlationId, retryCount, BuildFaultReason(exception));
        }
        catch (Exception persistEx)
        {
            // The dead-letter sink itself must never throw back into the fault/observer pipeline --
            // doing so could destabilise the bus's own fault handling. Log at Critical-equivalent
            // (Error) severity so the underlying persistence failure is still visible, but swallow
            // rather than propagate.
            logger.LogError(persistEx,
                "DeadLetterFaultObserver: failed to persist dead-letter row for {EventType} (original fault: {FaultReason}).",
                eventType, BuildFaultReason(exception));
        }
    }

    private static (string EventType, string SchemaVersion) ResolveDiscriminator<TMessage>(TMessage message)
        where TMessage : class
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

    private static string? SerializePayload<TMessage>(TMessage message)
        where TMessage : class
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

    private static string BuildFaultReason(Exception exception) =>
        $"{exception.GetType().Name}: {Truncate(exception.Message ?? string.Empty, 380)}";

    private static string BuildExceptionDetail(Exception exception) =>
        $"{exception.GetType().FullName}: {exception.Message}" +
        (string.IsNullOrEmpty(exception.StackTrace) ? string.Empty : $"\n{exception.StackTrace}");

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength] + "…";
}
