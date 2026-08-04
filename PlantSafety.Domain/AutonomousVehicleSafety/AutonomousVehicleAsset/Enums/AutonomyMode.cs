// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.AutonomousVehicleAsset
//  Layer:          Domain
//  File:           AutonomyMode.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Current operating mode of an AutonomousVehicleAsset at a point in time.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleAsset.Enums;

/// <summary>Current operating mode of an <see cref="Aggregates.AutonomousVehicleAsset"/>.</summary>
public enum AutonomyMode
{
    /// <summary>Under direct manual/human control.</summary>
    Manual = 0,

    /// <summary>Under remote human teleoperation.</summary>
    Teleoperated = 1,

    /// <summary>Operating autonomously with no direct human control.</summary>
    Autonomous = 2,
}
