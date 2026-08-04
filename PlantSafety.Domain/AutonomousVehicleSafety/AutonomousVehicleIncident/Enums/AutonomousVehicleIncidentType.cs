// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.AutonomousVehicleIncident
//  Layer:          Domain
//  File:           AutonomousVehicleIncidentType.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Classification of autonomous vehicle incident types.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleIncident.Enums;

/// <summary>Classification of autonomous vehicle incident types.</summary>
public enum AutonomousVehicleIncidentType
{
    /// <summary>A near-collision between an autonomous asset and another vehicle/asset.</summary>
    CollisionNearMiss = 0,

    /// <summary>A collision involving an autonomous asset.</summary>
    Collision = 1,

    /// <summary>A geofence boundary breach.</summary>
    GeofenceBreach = 2,

    /// <summary>A software/autonomy-stack fault.</summary>
    SoftwareFault = 3,

    /// <summary>Physical contact between a worker and an autonomous asset.</summary>
    HumanRobotContact = 4,
}
