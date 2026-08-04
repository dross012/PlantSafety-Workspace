// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.HumanRobotInteractionEvent
//  Layer:          Domain
//  File:           HumanRobotInteractionEventLifecycleRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the HumanRobotInteractionEvent aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.HumanRobotInteractionEvent.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="Aggregates.HumanRobotInteractionEvent"/> aggregate.</summary>
public static class HumanRobotInteractionEventLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when review may begin from <paramref name="current"/>.</summary>
    public static bool CanBeginReview(HumanRobotInteractionEventStatus current) => current == HumanRobotInteractionEventStatus.Recorded;

    /// <summary>Returns <see langword="true"/> when the event may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(HumanRobotInteractionEventStatus current) =>
        current is HumanRobotInteractionEventStatus.Recorded or HumanRobotInteractionEventStatus.UnderReview;
}
