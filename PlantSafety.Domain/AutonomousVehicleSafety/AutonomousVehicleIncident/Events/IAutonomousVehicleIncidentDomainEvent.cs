// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.AutonomousVehicleIncident
//  Layer:          Domain
//  File:           IAutonomousVehicleIncidentDomainEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all AutonomousVehicleIncident domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleIncident.Events;

/// <summary>Marker interface for all AutonomousVehicleIncident domain events.</summary>
public interface IAutonomousVehicleIncidentDomainEvent : IDomainEvent { }
