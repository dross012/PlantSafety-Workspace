// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Base
//  Layer:          SharedKernel
//  File:           AggregateRoot.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Abstract aggregate root base. Manages the in-process domain
//                  event list. Only aggregate root methods may raise domain events.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Abstractions;

namespace PlantSafety.SharedKernel.Base;

/// <summary>
/// Abstract aggregate root base with domain event management.
/// </summary>
/// <typeparam name="TId">Strong-typed identity of this aggregate root.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : struct
{
    private readonly List<Abstractions.IDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work, in causal order.</summary>
    public IReadOnlyCollection<Abstractions.IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected AggregateRoot() { }
    protected AggregateRoot(TId id) : base(id) { }

    /// <summary>Appends a domain event to the in-process event list.</summary>
    protected void RaiseDomainEvent(Abstractions.IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    /// <summary>Clears the in-process event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
