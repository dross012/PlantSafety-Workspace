// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.OperatingGeofence
//  Layer:          Domain
//  File:           IOperatingGeofenceDomainEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all OperatingGeofence domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.OperatingGeofence.Events;

/// <summary>Marker interface for all OperatingGeofence domain events.</summary>
public interface IOperatingGeofenceDomainEvent : IDomainEvent { }
