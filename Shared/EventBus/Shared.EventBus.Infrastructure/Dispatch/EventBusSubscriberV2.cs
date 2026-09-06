// ============================================================================
//  Project:        Shared.EventBus.Infrastructure
//  File:           EventBusSubscriberV2.cs
//  Created:        2026-08-08
//  Description:    Phase B.4: abstract base for a V2 subscriber bound to one
//                  consumer group, reading from an EventBusPublisherV2's
//                  in-memory queue. Deliberately simpler than V1's
//                  SqlEventSubscriber/EventHandlerRegistry machinery
//                  (retry/backoff/dead-letter over a durable SQL queue) --
//                  V2 has no durable store yet (see EventBusPublisherV2.cs),
//                  so replicating that machinery here would be dressing up
//                  an in-memory queue as more reliable than it is. A
//                  concrete subscriber overrides CanHandle/HandleAsync for
//                  whichever payload type(s) it owns.
// ============================================================================
using Shared.EventBus.Abstractions.Contracts;
using Shared.EventBus.Infrastructure.InMemory;

namespace Shared.EventBus.Infrastructure.Dispatch;

/// <summary>Abstract base for a V2 subscriber bound to one consumer group.</summary>
public abstract class EventBusSubscriberV2(string consumerGroup, EventBusPublisherV2 bus)
{
    private readonly HashSet<Guid> _processedEventIds = [];

    /// <summary>The consumer group this subscriber processes events for.</summary>
    public string ConsumerGroup { get; } = consumerGroup;

    /// <summary>Returns whether this subscriber handles the given envelope (typically a PayloadType check).</summary>
    protected abstract bool CanHandle(EventEnvelopeV2 envelope);

    /// <summary>Handles one envelope this subscriber has claimed via <see cref="CanHandle"/>. MUST be idempotent.</summary>
    protected abstract Task HandleAsync(EventEnvelopeV2 envelope, CancellationToken cancellationToken);

    /// <summary>Processes every unprocessed, claimable envelope currently published, up to <paramref name="maxBatchSize"/>. Returns the number processed.</summary>
    public async Task<int> ProcessDueEventsAsync(int maxBatchSize = 50, CancellationToken cancellationToken = default)
    {
        var processedCount = 0;
        foreach (EventEnvelopeV2 envelope in bus.PublishedEvents)
        {
            if (processedCount >= maxBatchSize)
            {
                break;
            }

            if (_processedEventIds.Contains(envelope.EventId) || !CanHandle(envelope))
            {
                continue;
            }

            await HandleAsync(envelope, cancellationToken);
            _processedEventIds.Add(envelope.EventId);
            processedCount++;
        }

        return processedCount;
    }
}
