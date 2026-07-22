// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety
//  Layer:          Domain
//  File:           IMachineSafetyDomainEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Marker interface for top-level MachineSafety domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.Events;

/// <summary>Marker interface for top-level MachineSafety domain events.</summary>
public interface IMachineSafetyDomainEvent : IDomainEvent { }