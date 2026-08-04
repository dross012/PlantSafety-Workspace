// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.AutonomousVehicleAsset
//  Layer:          Domain
//  File:           AutonomousVehicleAssetArchivalPolicy.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for AutonomousVehicleAsset aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleAsset.Policies;

/// <summary>Determines whether an <see cref="Aggregates.AutonomousVehicleAsset"/> aggregate is eligible for archival.</summary>
public static class AutonomousVehicleAssetArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the asset record may be archived (retired only).</summary>
    public static bool IsEligibleForArchival(Aggregates.AutonomousVehicleAsset aggregate)
        => aggregate.Status == AutonomousVehicleAssetStatus.Retired;
}
