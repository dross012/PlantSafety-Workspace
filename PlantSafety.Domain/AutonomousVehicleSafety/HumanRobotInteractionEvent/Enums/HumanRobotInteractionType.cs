// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.HumanRobotInteractionEvent
//  Layer:          Domain
//  File:           HumanRobotInteractionType.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Classification of human-robot interaction event types.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.HumanRobotInteractionEvent.Enums;

/// <summary>Classification of human-robot interaction event types.</summary>
public enum HumanRobotInteractionType
{
    /// <summary>A worker entered the asset's proximity-detection zone.</summary>
    ProximityAlert = 0,

    /// <summary>A near-miss between a worker and the asset.</summary>
    NearMiss = 1,

    /// <summary>Physical contact occurred between a worker and the asset.</summary>
    PhysicalContact = 2,

    /// <summary>The asset's emergency stop was triggered due to a human presence.</summary>
    EmergencyStopTriggered = 3,
}
