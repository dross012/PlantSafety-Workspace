// ============================================================================
//  Project:        Shared.EventBus.Infrastructure
//  File:           EventBusPublisherV2.cs
//  Created:        2026-08-08
//  Description:    Phase B.4: in-memory IEventPublisherV2, mirroring
//                  InMemoryEventBus's role for V1 -- not durable, not for
//                  production use. A SQL-backed V2 publisher (extending
//                  dbo.EventQueue with the new nullable enrichment columns,
//                  or a dedicated V2 table) is a real, separate decision
//                  deliberately deferred rather than made silently here;
//                  see this phase's own notes on why B.4 stopped short of
//                  altering the live dbo.EventQueue table.
// ============================================================================
using System.Collections.Concurrent;

using Shared.EventBus.Abstractions.Contracts;

namespace Shared.EventBus.Infrastructure.InMemory;

/// <summary>In-memory, single-process <see cref="IEventPublisherV2"/>, for tests and local dev.</summary>
public sealed class EventBusPublisherV2 : IEventPublisherV2
{
    private readonly ConcurrentQueue<EventEnvelopeV2> _queue = new();

    /// <summary>All V2 envelopes published so far, in publish order. Test/inspection convenience, and the source subscribers read from.</summary>
    public IReadOnlyCollection<EventEnvelopeV2> PublishedEvents => _queue.ToArray();

    /// <inheritdoc />
    public Task PublishAsync(EventEnvelopeV2 envelope, CancellationToken cancellationToken = default)
    {
        _queue.Enqueue(envelope);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task PublishAsync<TPayload>(
        TPayload payload,
        string tenantId,
        Guid correlationId,
        Guid causationId,
        DateTime timestampUtc,
        string sourceModule,
        string eventType,
        int schemaVersion,
        Guid? facilityId = null,
        string? industryName = null,
        string? location = null,
        string? hazardType = null,
        CancellationToken cancellationToken = default)
        where TPayload : notnull
    {
        EventEnvelopeV2 envelope = EventEnvelopeV2.Create(
            payload, tenantId, correlationId, causationId, timestampUtc, sourceModule, eventType, schemaVersion,
            facilityId: facilityId, industryName: industryName, location: location, hazardType: hazardType);
        return PublishAsync(envelope, cancellationToken);
    }
}
