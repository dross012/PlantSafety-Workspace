// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.TwoHandControls
//  Layer:          Domain
//  File:           ITwoHandControlsDomainEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all TwoHandControls domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.TwoHandControls.Events;

/// <summary>Marker interface for all TwoHandControls domain events.</summary>
public interface ITwoHandControlsDomainEvent : IDomainEvent { }