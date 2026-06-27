// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Abstractions
//  Layer:          SharedKernel
//  File:           IAggregateRoot.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Marker contract for all aggregate roots in the domain.
//                  Aggregate roots are the sole entry points for state mutation.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.SharedKernel.Abstractions;

/// <summary>
/// Marker contract for all aggregate roots.
/// An aggregate root is the sole entry point for state mutations within its boundary.
/// </summary>
public interface IAggregateRoot : IEntity, IHasDomainEvents { }
