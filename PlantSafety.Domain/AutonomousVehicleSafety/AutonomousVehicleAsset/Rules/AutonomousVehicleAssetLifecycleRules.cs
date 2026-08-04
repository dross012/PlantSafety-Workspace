// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.AutonomousVehicleAsset
//  Layer:          Domain
//  File:           AutonomousVehicleAssetLifecycleRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the AutonomousVehicleAsset aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleAsset.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="Aggregates.AutonomousVehicleAsset"/> aggregate.</summary>
public static class AutonomousVehicleAssetLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when an autonomy-mode transition may be recorded from <paramref name="current"/>.</summary>
    public static bool CanRecordModeTransition(AutonomousVehicleAssetStatus current) =>
        current is AutonomousVehicleAssetStatus.Registered or AutonomousVehicleAssetStatus.Active;

    /// <summary>Returns <see langword="true"/> when autonomous operation may be suspended from <paramref name="current"/>.</summary>
    public static bool CanSuspend(AutonomousVehicleAssetStatus current) => current == AutonomousVehicleAssetStatus.Active;

    /// <summary>Returns <see langword="true"/> when a suspended asset may be reactivated from <paramref name="current"/>.</summary>
    public static bool CanReactivate(AutonomousVehicleAssetStatus current) => current == AutonomousVehicleAssetStatus.Suspended;

    /// <summary>Returns <see langword="true"/> when the asset may be retired from <paramref name="current"/>.</summary>
    public static bool CanRetire(AutonomousVehicleAssetStatus current) => current != AutonomousVehicleAssetStatus.Retired;
}
