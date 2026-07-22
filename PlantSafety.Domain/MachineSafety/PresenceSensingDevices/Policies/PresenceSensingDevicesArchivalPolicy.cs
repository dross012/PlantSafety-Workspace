// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.PresenceSensingDevices
//  Layer:          Domain
//  File:           PresenceSensingDevicesArchivalPolicy.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain policy governing archival eligibility for PresenceSensingDevices aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.PresenceSensingDevices.Policies;

/// <summary>Determines whether a <see cref="PresenceSensingDevices"/> aggregate is eligible for archival.</summary>
public static class PresenceSensingDevicesArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the device record may be archived (retired only).</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.MachineSafety.PresenceSensingDevices.Aggregates.PresenceSensingDevices aggregate)
        => aggregate.Status == PresenceSensingDevicesStatus.Retired;
}
