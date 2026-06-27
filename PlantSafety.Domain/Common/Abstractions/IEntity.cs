// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Common
//  Layer:          Domain
//  File:           IEntity.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//      Canonical interface for all domain entities. Entities have identity
//      (TId) and equality is determined by that identity, not by value.
//      Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Common.Abstractions;

/// <summary>Marker interface for domain entities keyed by <typeparamref name="TId"/>.</summary>
public interface IEntity<out TId>
{
    /// <summary>Strong-typed identity of this entity.</summary>
    TId Id { get; }
}
