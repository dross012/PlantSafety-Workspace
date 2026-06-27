// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Common
//  Layer:          Domain
//  File:           AggregateRoot.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//      Optional abstract base class for aggregate roots. Manages the internal
//      domain-event list and satisfies IAggregateRoot<TId>.
//      Aggregates that need special event typing (e.g. IAlertDomainEvent) may
//      choose to implement the pattern directly rather than inheriting here.
//      Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Common.Base;

/// <summary>
/// Optional base class for aggregate roots. Provides domain-event collection
/// management and satisfies <see cref="IAggregateRoot{TId}"/>.
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot<TId>
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <inheritdoc/>
    public IReadOnlyCollection<IDomainEvent> DomainEvents
        => new ReadOnlyCollection<IDomainEvent>(_domainEvents);

    /// <summary>For ORM/rehydration only.</summary>
    protected AggregateRoot() { }

    /// <summary>Initialises with explicit identity.</summary>
    protected AggregateRoot(TId id) : base(id) { }

    /// <summary>Enqueues a domain event for dispatch after the UoW commits.</summary>
    protected void AddDomainEvent(IDomainEvent domainEvent)
        => _domainEvents.Add(domainEvent);

    /// <inheritdoc/>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
