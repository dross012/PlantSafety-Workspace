// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Common
//  Layer:          Domain
//  File:           IAggregateRoot.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//      Combines entity identity and domain-event publication into a single
//      aggregate root contract. All aggregate roots must satisfy this interface.
//      Cross-aggregate references use StrongId only — never aggregate instances.
//      Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Common.Abstractions;

/// <summary>
/// Canonical contract for aggregate roots. Combines <see cref="IEntity{TId}"/>
/// identity with <see cref="IHasDomainEvents"/> publication.
/// </summary>
public interface IAggregateRoot<out TId> : IEntity<TId>, IHasDomainEvents
{
}
