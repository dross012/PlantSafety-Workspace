// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Shared result payload for HumanRobotInteractionEvent commands ("AutonomousVehicleSafety.HumanRobotInteractionEvent").
// SchemaVersion: 1

namespace PlantSafety.Application.AutonomousVehicleSafety.Commands;

/// <summary>Result of a successful HumanRobotInteractionEvent command execution.</summary>
/// <param name="HumanRobotInteractionEventId">Identity of the affected HumanRobotInteractionEvent aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record HumanRobotInteractionEventActionResult(
    Guid HumanRobotInteractionEventId,
    string Status);
