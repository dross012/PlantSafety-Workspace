// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.AutonomousVehicleAsset
//  Layer:          Domain
//  File:           AutonomousVehicleAssetStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the AutonomousVehicleAsset aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleAsset.Enums;

/// <summary>Lifecycle states for the <see cref="Aggregates.AutonomousVehicleAsset"/> aggregate root.</summary>
public enum AutonomousVehicleAssetStatus
{
    /// <summary>Asset registered in the fleet registry but not yet placed into service.</summary>
    Registered = 0,

    /// <summary>Asset is actively operating on-site.</summary>
    Active = 1,

    /// <summary>Autonomous operation has been suspended pending review.</summary>
    Suspended = 2,

    /// <summary>Asset permanently retired/decommissioned.</summary>
    Retired = 3,
}
