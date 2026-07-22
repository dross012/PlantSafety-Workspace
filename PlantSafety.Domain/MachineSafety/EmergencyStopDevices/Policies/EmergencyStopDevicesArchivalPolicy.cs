// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.EmergencyStopDevices
//  Layer:          Domain
//  File:           EmergencyStopDevicesArchivalPolicy.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain policy governing archival eligibility for EmergencyStopDevices aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.EmergencyStopDevices.Policies;

/// <summary>Determines whether a <see cref="EmergencyStopDevices"/> aggregate is eligible for archival.</summary>
public static class EmergencyStopDevicesArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the device record may be archived (retired only).</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.MachineSafety.EmergencyStopDevices.Aggregates.EmergencyStopDevices aggregate)
        => aggregate.Status == EmergencyStopDevicesStatus.Retired;
}
