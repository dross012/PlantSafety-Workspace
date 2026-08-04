// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.OperatingGeofence
//  Layer:          Domain
//  File:           OperatingGeofenceStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the OperatingGeofence aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.OperatingGeofence.Enums;

/// <summary>Lifecycle states for the <see cref="Aggregates.OperatingGeofence"/> aggregate root.</summary>
public enum OperatingGeofenceStatus
{
    /// <summary>Geofence boundary defined but not yet enforced.</summary>
    Draft = 0,

    /// <summary>Geofence is actively enforced.</summary>
    Active = 1,

    /// <summary>Geofence is under audit following a site layout change.</summary>
    UnderAudit = 2,

    /// <summary>Geofence retired and no longer enforced.</summary>
    Retired = 3,
}
