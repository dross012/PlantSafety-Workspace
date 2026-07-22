// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Persistence
//  Layer:          Infrastructure
//  File:           OutboxWriter.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    IOutboxWriter implementation that serialises domain events and writes them to OutboxMessages.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Persistence.UnitOfWork;

/// <summary>
/// <see cref="IOutboxWriter"/> that serialises domain events to canonical envelopes
/// and writes them to the <c>System.OutboxMessages</c> table within the active UoW transaction.
/// </summary>
/// <remarks>
/// Registered <c>AddScoped</c> (see <c>InfrastructureServiceRegistration</c>), so a single instance
/// is shared for the lifetime of one HTTP request / one MediatR pipeline execution. Repositories
/// that resolve <see cref="IOutboxWriter"/> and <see cref="TransactionalUnitOfWork"/> within the
/// same DI scope will therefore observe the same pending-events list.
/// </remarks>
public sealed class OutboxWriter(ISqlConnectionFactory connectionFactory, DapperCommandExecutor commandExecutor, ILogger<OutboxWriter> logger) : IOutboxWriter
{
    private readonly List<OutboxEnvelope> _pending = [];

    /// <inheritdoc/>
    public bool HasPendingEvents => _pending.Count > 0;

    /// <summary>Enqueues a domain event for outbox writing.</summary>
    public void Enqueue(IDomainEvent domainEvent, string tenantId, string actorId, string correlationId)
    {
        var envelope = new OutboxEnvelope
        {
            MessageId     = Uuid7.New(),
            EventType     = domainEvent.EventType,
            SchemaVersion = domainEvent.SchemaVersion,
            TenantId      = tenantId,
            ActorId       = actorId,
            CorrelationId = correlationId,
            OccurredAt    = domainEvent.OccurredAtUtc.Value,
            Payload       = JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), JsonSerializerConfiguration.Default),
        };

        _pending.Add(envelope);
    }

    /// <inheritdoc/>
    public async Task FlushAsync(CancellationToken cancellationToken = default)
    {
        if (_pending.Count == 0) return;
        logger.LogDebug("OutboxWriter: flushing {Count} events.", _pending.Count);

        using var connection = connectionFactory.CreateConnection();

        foreach (var envelope in _pending)
        {
            // NOTE: the Dapper parameter is deliberately named "EnvelopeTenantId", NOT "TenantId".
            // DapperCommandExecutor.ExecuteAsync calls parameters.GetTenantId() for RLS defense-in-
            // depth on every call, which does `parameters.Get<Guid>("TenantId")` on any parameter
            // literally named "TenantId". The outbox envelope's TenantId is a free-form string (see
            // OutboxEnvelope / IUnitOfWork.EnqueueEvent's string tenantId), not a Guid, so using the
            // literal name "TenantId" here throws InvalidCastException. [System].[OutboxMessages]
            // has no RLS predicate (it is a cross-tenant system table), so no session-context call
            // is needed for this procedure anyway.
            var parameters = new DynamicParameters();
            parameters.Add("MessageId", envelope.MessageId);
            parameters.Add("EventType", envelope.EventType);
            parameters.Add("SchemaVersion", envelope.SchemaVersion);
            parameters.Add("EnvelopeTenantId", envelope.TenantId);
            parameters.Add("ActorId", envelope.ActorId);
            parameters.Add("CorrelationId", envelope.CorrelationId);
            parameters.Add("OccurredAt", envelope.OccurredAt);
            parameters.Add("Payload", envelope.Payload);

            await commandExecutor.ExecuteAsync(
                connection,
                "[System].[usp_OutboxMessages_Insert]",
                parameters,
                cancellationToken: cancellationToken);
        }

        logger.LogDebug("OutboxWriter: flushed {Count} events to [System].[OutboxMessages].", _pending.Count);
        _pending.Clear();
    }

    /// <inheritdoc/>
    public void Clear()
    {
        if (_pending.Count == 0) return;
        logger.LogDebug("OutboxWriter: discarding {Count} pending events (rollback).", _pending.Count);
        _pending.Clear();
    }
}

/// <summary>Canonical outbox envelope serialised to <c>System.OutboxMessages</c>.</summary>
public sealed record OutboxEnvelope
{
    public Guid MessageId     { get; init; }
    public string EventType   { get; init; } = string.Empty;
    public string SchemaVersion { get; init; } = string.Empty;
    public string TenantId    { get; init; } = string.Empty;
    public string ActorId     { get; init; } = string.Empty;
    public string CorrelationId { get; init; } = string.Empty;
    public DateTime OccurredAt { get; init; }
    public string Payload     { get; init; } = string.Empty;
    public bool Processed     { get; init; }
    public DateTime? ProcessedAt { get; init; }
}
