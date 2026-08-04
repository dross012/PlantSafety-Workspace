// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.OperatingGeofence
//  Layer:          Domain
//  File:           OperatingGeofenceArchivalPolicy.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for OperatingGeofence aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.OperatingGeofence.Policies;

/// <summary>Determines whether an <see cref="Aggregates.OperatingGeofence"/> aggregate is eligible for archival.</summary>
public static class OperatingGeofenceArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the geofence record may be archived (retired only).</summary>
    public static bool IsEligibleForArchival(Aggregates.OperatingGeofence aggregate)
        => aggregate.Status == OperatingGeofenceStatus.Retired;
}
