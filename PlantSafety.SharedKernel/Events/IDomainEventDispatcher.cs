// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Events
//  Layer:          SharedKernel
//  File:           IDomainEventDispatcher.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Contract for dispatching domain events after a successful
//                  Unit of Work commit. Implementation lives in Infrastructure.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Abstractions;

namespace PlantSafety.SharedKernel.Events;

/// <summary>
/// Dispatches domain events collected by aggregate roots.
/// Called by the Unit of Work after a successful transaction commit.
/// Implementations MUST be idempotent: duplicate dispatch MUST be a no-op.
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// Dispatches all domain events from the given aggregate and clears the event list.
    /// </summary>
    Task DispatchAndClearAsync(
        IHasDomainEvents aggregate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Dispatches a single domain event.
    /// </summary>
    Task DispatchAsync(
        IDomainEvent domainEvent,
        CancellationToken cancellationToken = default);
}
