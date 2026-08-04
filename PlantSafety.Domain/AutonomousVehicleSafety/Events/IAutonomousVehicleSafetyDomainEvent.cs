// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety
//  Layer:          Domain
//  File:           IAutonomousVehicleSafetyDomainEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Marker interface for top-level AutonomousVehicleSafety domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.Events;

/// <summary>Marker interface for top-level AutonomousVehicleSafety domain events.</summary>
public interface IAutonomousVehicleSafetyDomainEvent : IDomainEvent { }
