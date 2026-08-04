// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.HumanRobotInteractionEvent
//  Layer:          Domain
//  File:           InteractionSeverityLevel.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Severity classification for a recorded human-robot interaction event.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.HumanRobotInteractionEvent.Enums;

/// <summary>Severity classification for a recorded human-robot interaction event.</summary>
public enum InteractionSeverityLevel
{
    /// <summary>Low severity; informational.</summary>
    Low = 0,

    /// <summary>Medium severity; warrants review.</summary>
    Medium = 1,

    /// <summary>High severity; warrants prompt investigation.</summary>
    High = 2,

    /// <summary>Critical severity; immediate escalation required.</summary>
    Critical = 3,
}
