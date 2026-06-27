// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Abstractions
//  Layer:          SharedKernel
//  File:           IEntity.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Marker contract for all domain entities.
//                  Entities are distinguished by identity, not attribute equality.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.SharedKernel.Abstractions;

/// <summary>
/// Marker contract for all domain entities.
/// Entities have a stable identity that persists across state changes.
/// </summary>
public interface IEntity { }
