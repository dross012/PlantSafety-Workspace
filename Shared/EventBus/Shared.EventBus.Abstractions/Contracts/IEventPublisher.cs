// ============================================================================
//  Project:        Shared.EventBus.Abstractions
//  File:           IEventPublisher.cs
//  Created:        2026-07-27
//  Description:    Publisher-side abstraction of the cross-module event bus. Publishing an
//                  envelope enqueues it durably (SQL queue table, or in-memory for tests) --
//                  it does not guarantee synchronous delivery to subscribers. Consumers pull
//                  and process independently (see IEventSubscriber / dispatcher).
// ============================================================================
namespace Shared.EventBus.Abstractions.Contracts;

/// <summary>Publishes cross-module integration events onto the shared event bus.</summary>
public interface IEventPublisher
{
    /// <summary>
    /// Durably enqueues the given envelope for delivery to all registered subscribers.
    /// Must be called only after the originating transaction has committed
    /// (Publish-After-Commit guarantee, patterns.md §6) to avoid publishing phantom events.
    /// </summary>
    Task PublishAsync(EventEnvelope envelope, CancellationToken cancellationToken = default);

    /// <summary>Convenience overload: builds and publishes an envelope from a typed DTO.</summary>
    Task PublishAsync<TPayload>(
        TPayload payload,
        string tenantId,
        Guid correlationId,
        Guid causationId,
        DateTime occurredAtUtc,
        string sourceModule,
        string eventType,
        int schemaVersion,
        CancellationToken cancellationToken = default)
        where TPayload : notnull;
}
