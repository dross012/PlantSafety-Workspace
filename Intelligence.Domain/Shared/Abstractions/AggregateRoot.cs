/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               AggregateRoot.cs
    DESCRIPTION:        Base class for all Aggregate Roots in the Intelligence domain. Maintains a
                        private collection of domain events raised during the aggregate's lifetime.
                        The Unit of Work drains events via ClearDomainEvents() after writing them to
                        the Outbox in the same database transaction. Events are never dispatched in-process.
                        (Rule 2.5, Outbox pattern)

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using Intelligence.Domain.Shared.Interfaces;

namespace Intelligence.Domain.Shared.Abstractions;

/// <summary>
/// Base class for Intelligence domain Aggregate Roots. Owns the domain event collection.
/// Only aggregate roots may call <see cref="RaiseDomainEvent"/>.
/// </summary>
/// <typeparam name="TId">The strongly-typed identifier type. Must be a struct.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : struct
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>Domain events raised during this aggregate's current unit of work.</summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected AggregateRoot(TId id) : base(id) { }

    /// <summary>
    /// Raises a domain event. Called only from within aggregate root methods.
    /// Events are written to the Outbox by the Unit of Work before being cleared.
    /// </summary>
    protected void RaiseDomainEvent(IDomainEvent domainEvent) =>
        _domainEvents.Add(domainEvent);

    /// <summary>
    /// Clears all pending domain events. Called by the Unit of Work after events have been
    /// written to the Outbox in the same database transaction.
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
