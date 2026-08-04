// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.HumanRobotInteractionEvent
//  Layer:          Domain
//  File:           HumanRobotInteractionEventStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the HumanRobotInteractionEvent aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.HumanRobotInteractionEvent.Enums;

/// <summary>Lifecycle states for the <see cref="Aggregates.HumanRobotInteractionEvent"/> aggregate root.</summary>
public enum HumanRobotInteractionEventStatus
{
    /// <summary>Interaction event recorded, awaiting review.</summary>
    Recorded = 0,

    /// <summary>Interaction event is under safety review.</summary>
    UnderReview = 1,

    /// <summary>Interaction event review is closed.</summary>
    Closed = 2,
}
