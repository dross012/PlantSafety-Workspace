// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.HumanRobotInteractionEvent
//  Layer:          Domain
//  File:           HumanRobotInteractionEventArchivalPolicy.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for HumanRobotInteractionEvent
//                  aggregates. Interaction/telemetry data is retained per the module's
//                  incident-investigation retention policy, then purged. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.HumanRobotInteractionEvent.Policies;

/// <summary>Determines whether a <see cref="Aggregates.HumanRobotInteractionEvent"/> aggregate is eligible for archival.</summary>
public static class HumanRobotInteractionEventArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the interaction record may be archived (closed only).</summary>
    public static bool IsEligibleForArchival(Aggregates.HumanRobotInteractionEvent aggregate)
        => aggregate.Status == HumanRobotInteractionEventStatus.Closed;
}
