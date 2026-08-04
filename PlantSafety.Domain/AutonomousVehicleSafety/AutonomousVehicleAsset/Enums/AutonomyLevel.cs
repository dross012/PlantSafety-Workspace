// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.AutonomousVehicleAsset
//  Layer:          Domain
//  File:           AutonomyLevel.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Rated degree of autonomy for an AutonomousVehicleAsset, loosely aligned to the
//                  ANSI/RIA R15.08 / ISO 3691-4 concept of autonomy tiers.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleAsset.Enums;

/// <summary>Rated degree of autonomy for an <see cref="Aggregates.AutonomousVehicleAsset"/>.</summary>
public enum AutonomyLevel
{
    /// <summary>Operator-assisted; automation limited to advisory/braking assistance.</summary>
    Assisted = 0,

    /// <summary>Semi-autonomous; automation performs the task under active human supervision.</summary>
    SemiAutonomous = 1,

    /// <summary>Highly autonomous; automation performs the task with human oversight on exceptions only.</summary>
    HighlyAutonomous = 2,

    /// <summary>Fully autonomous; no human oversight required during normal operation.</summary>
    FullyAutonomous = 3,
}
