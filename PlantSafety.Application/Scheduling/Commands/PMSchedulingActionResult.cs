// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Shared result payload for PMScheduling lifecycle commands (schedule, record
// completion, escalate, cancel).
// SchemaVersion: 1

namespace PlantSafety.Application.Scheduling.Commands;

/// <summary>Result of a successful PMScheduling lifecycle command execution.</summary>
/// <param name="PMSchedulingId">Identity of the affected PMScheduling aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record PMSchedulingActionResult(
    Guid PMSchedulingId,
    string Status);
