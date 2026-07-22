// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Shared result payload for PPEAssignment lifecycle commands (assign, record
// fit test, flag for replacement, retire).
// SchemaVersion: 1

namespace PlantSafety.Application.PPE.Commands;

/// <summary>Result of a successful PPEAssignment lifecycle command execution.</summary>
/// <param name="PPEAssignmentId">Identity of the affected PPEAssignment aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record PPEAssignmentActionResult(
    Guid PPEAssignmentId,
    string Status);
