// ============================================================================
//  Project:        Shared.EventBus.Abstractions
//  File:           IEventHandler.cs
//  Created:        2026-07-27
//  Description:    Consumer-side handler contract. One handler = one idempotent side effect
//                  for one payload type within one consumer group (module). Multiple modules
//                  may each register their own IEventHandler<TPayload> for the same event type
//                  without interfering with each other (idempotency is tracked per
//                  ConsumerGroup+EventId, see Shared.EventBus.Infrastructure ProcessedEvents).
// ============================================================================
namespace Shared.EventBus.Abstractions.Handlers;

/// <summary>Handles a single cross-module integration event payload type.</summary>
/// <typeparam name="TPayload">The deserialized integration event DTO type.</typeparam>
public interface IEventHandler<in TPayload> where TPayload : notnull
{
    /// <summary>
    /// Logical name of the consumer group this handler belongs to (e.g. "Reporting").
    /// Used as the idempotency/dead-letter partition key alongside EventId.
    /// </summary>
    string ConsumerGroup { get; }

    /// <summary>
    /// Handles the event. MUST be idempotent -- may be invoked more than once for the same
    /// EventId under at-least-once delivery. MUST NOT throw for expected/business-level
    /// conditions; only throw for genuine processing failures that should be retried.
    /// </summary>
    Task HandleAsync(TPayload payload, Contracts.EventEnvelope envelope, CancellationToken cancellationToken);
}
