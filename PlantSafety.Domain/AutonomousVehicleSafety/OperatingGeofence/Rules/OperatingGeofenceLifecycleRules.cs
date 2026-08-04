// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.OperatingGeofence
//  Layer:          Domain
//  File:           OperatingGeofenceLifecycleRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the OperatingGeofence aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.OperatingGeofence.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="Aggregates.OperatingGeofence"/> aggregate.</summary>
public static class OperatingGeofenceLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the geofence may be activated from <paramref name="current"/>.</summary>
    public static bool CanActivate(OperatingGeofenceStatus current) => current == OperatingGeofenceStatus.Draft;

    /// <summary>Returns <see langword="true"/> when a violation may be recorded from <paramref name="current"/>.</summary>
    public static bool CanRecordViolation(OperatingGeofenceStatus current) => current == OperatingGeofenceStatus.Active;

    /// <summary>Returns <see langword="true"/> when an audit may be scheduled/recorded from <paramref name="current"/>.</summary>
    public static bool CanAudit(OperatingGeofenceStatus current) =>
        current is OperatingGeofenceStatus.Active or OperatingGeofenceStatus.UnderAudit;

    /// <summary>Returns <see langword="true"/> when the geofence may be retired from <paramref name="current"/>.</summary>
    public static bool CanRetire(OperatingGeofenceStatus current) => current != OperatingGeofenceStatus.Retired;
}
