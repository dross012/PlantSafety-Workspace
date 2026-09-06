// ============================================================================
//  Project:        Shared.EventBus.Abstractions
//  File:           IEventSubscriber.cs
//  Created:        2026-07-27
//  Description:    Consumer-side abstraction that pulls due envelopes for a given consumer
//                  group and dispatches them to the matching IEventHandler<TPayload>.
//                  Implemented by Shared.EventBus.Infrastructure (SQL-queue-backed) and by
//                  the in-memory test double.
// ============================================================================
namespace Shared.EventBus.Abstractions.Handlers;

/// <summary>Pulls and dispatches due events for one consumer group.</summary>
public interface IEventSubscriber
{
    /// <summary>Logical name of the consumer group this subscriber processes on behalf of.</summary>
    string ConsumerGroup { get; }

    /// <summary>
    /// Processes up to <paramref name="maxBatchSize"/> due envelopes for this consumer group:
    /// idempotency check, handler dispatch, retry/backoff bookkeeping, and dead-letter routing
    /// on poison messages. Returns the number of envelopes processed (successfully or dead-lettered).
    /// </summary>
    Task<int> ProcessDueEventsAsync(int maxBatchSize = 50, CancellationToken cancellationToken = default);
}
