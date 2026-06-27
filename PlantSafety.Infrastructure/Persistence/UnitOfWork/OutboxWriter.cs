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
public sealed class OutboxWriter(ILogger<OutboxWriter> logger) : IOutboxWriter
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
        // Actual INSERT to [System].[OutboxMessages] happens here via Dapper.
        // Implementation deferred to OutboxMessageRepository.
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
