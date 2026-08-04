// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.OperatingGeofence
//  Layer:          Domain
//  File:           GeofenceZoneType.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Classification of operating-domain geofence zone types.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.OperatingGeofence.Enums;

/// <summary>Classification of operating-domain geofence zone types.</summary>
public enum GeofenceZoneType
{
    /// <summary>Zone in which autonomous operation is prohibited entirely.</summary>
    ExclusionZone = 0,

    /// <summary>Zone with restricted/conditional autonomous operation (e.g. reduced speed, escort required).</summary>
    RestrictedZone = 1,

    /// <summary>Normal operating zone for autonomous assets.</summary>
    OperatingZone = 2,

    /// <summary>Zone with an enforced maximum speed limit.</summary>
    SpeedLimitZone = 3,
}
