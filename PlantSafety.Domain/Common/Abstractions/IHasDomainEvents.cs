// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Common
//  Layer:          Domain
//  File:           IHasDomainEvents.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//      Applied to aggregate roots that raise domain events. The infrastructure
//      layer reads DomainEvents after the UoW commits and dispatches them via
//      the outbox pattern.
//      Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Common.Abstractions;

/// <summary>Implemented by aggregates that accumulate and expose domain events.</summary>
public interface IHasDomainEvents
{
    /// <summary>Events raised during this unit-of-work; read by the dispatcher after commit.</summary>
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    /// <summary>Clears the in-memory event list after dispatch. Called by the dispatcher.</summary>
    void ClearDomainEvents();
}
