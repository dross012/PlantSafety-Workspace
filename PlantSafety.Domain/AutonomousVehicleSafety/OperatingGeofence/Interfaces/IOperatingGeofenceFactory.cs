// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.OperatingGeofence
//  Layer:          Domain
//  File:           IOperatingGeofenceFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing OperatingGeofence aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.OperatingGeofence.Interfaces;

/// <summary>Factory interface for constructing <see cref="Aggregates.OperatingGeofence"/> aggregates.</summary>
public interface IOperatingGeofenceFactory
{
    /// <summary>Creates a new OperatingGeofence aggregate with the supplied parameters.</summary>
    Aggregates.OperatingGeofence Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}
