// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Integration event publishing abstraction (Outbox pattern).
//   Events are published AFTER successful commit only (Publish-After-Commit Rule, §4).
//   The Infrastructure layer provides the MassTransit/Azure Service Bus implementation.
//   Application layer code MUST NOT call this interface directly — only via Unit of Work.
// SchemaVersion: 1
// Additive-only evolution: new members may be added; no members may be removed or renamed.

using PlantSafety.Application.Abstractions.Messaging;

namespace PlantSafety.Application.Abstractions.Events;

/// <summary>
/// Publishes integration events to the message bus after successful persistence.
/// This interface is invoked only by the Unit of Work as part of the atomic commit sequence.
/// Application handlers MUST NOT call this interface directly; they raise domain events on aggregates.
/// </summary>
/// <remarks>
/// Canonical event flow:
/// 1. Aggregate raises domain events
/// 2. Unit of Work collects events
/// 3. Before commit, each event is wrapped in a canonical envelope and written to OutboxMessages
/// 4. Outbox Processor reads envelopes and invokes <see cref="PublishAsync{TEvent}"/>
/// 5. On failure, error envelope is emitted; no partial publish
/// </remarks>
public interface IIntegrationEventPublisher
{
    /// <summary>
    /// Publishes a single integration event to the message bus.
    /// This method is called by the Outbox Processor after successful persistence, never by handlers.
    /// </summary>
    /// <typeparam name="TEvent">The integration event type.</typeparam>
    /// <param name="integrationEvent">The event to publish.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent;

    /// <summary>
    /// Publishes a batch of integration events atomically. All events must share the same CorrelationId.
    /// </summary>
    /// <param name="events">The events to publish in batch.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task PublishBatchAsync(IReadOnlyList<IIntegrationEvent> events, CancellationToken cancellationToken = default);
}
