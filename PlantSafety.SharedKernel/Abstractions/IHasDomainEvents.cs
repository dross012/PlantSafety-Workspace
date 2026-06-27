// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Abstractions
//  Layer:          SharedKernel
//  File:           IHasDomainEvents.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Contract for aggregates that raise and hold domain events.
//                  Events are cleared after dispatch by the UnitOfWork.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.SharedKernel.Abstractions;

/// <summary>
/// Contract for types that collect and expose domain events.
/// Domain events MUST be cleared after dispatch (via <see cref="ClearDomainEvents"/>).
/// </summary>
public interface IHasDomainEvents
{
    /// <summary>Domain events raised during this unit of work, in causal order.</summary>
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    void ClearDomainEvents();
}
