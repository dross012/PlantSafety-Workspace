// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.CraneHoistSystems
//  Layer:          Domain
//  File:           ICraneHoistSystemsDomainEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Marker interface for all CraneHoistSystems domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.CraneHoistSystems.Events;

/// <summary>Marker interface for all CraneHoistSystems domain events.</summary>
public interface ICraneHoistSystemsDomainEvent : IDomainEvent { }
