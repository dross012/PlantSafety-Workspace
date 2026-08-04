// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.AutonomousVehicleAsset
//  Layer:          Domain
//  File:           AutonomousVehicleType.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Classification of autonomous/semi-autonomous vehicle and mobile-robot asset types.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleAsset.Enums;

/// <summary>Classification of autonomous/semi-autonomous vehicle and mobile-robot asset types.</summary>
public enum AutonomousVehicleType
{
    /// <summary>Automated Guided Vehicle (fixed-path, wire/tape/laser guided).</summary>
    Agv = 0,

    /// <summary>Autonomous Mobile Robot (dynamic path planning).</summary>
    Amr = 1,

    /// <summary>Autonomous yard truck / terminal tractor.</summary>
    AutonomousYardTruck = 2,
}
