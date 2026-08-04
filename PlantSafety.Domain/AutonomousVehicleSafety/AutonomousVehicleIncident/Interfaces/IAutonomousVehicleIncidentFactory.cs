// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.AutonomousVehicleIncident
//  Layer:          Domain
//  File:           IAutonomousVehicleIncidentFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing AutonomousVehicleIncident aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleIncident.Interfaces;

/// <summary>Factory interface for constructing <see cref="Aggregates.AutonomousVehicleIncident"/> aggregates.</summary>
public interface IAutonomousVehicleIncidentFactory
{
    /// <summary>Creates a new AutonomousVehicleIncident aggregate with the supplied parameters.</summary>
    Aggregates.AutonomousVehicleIncident Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}
