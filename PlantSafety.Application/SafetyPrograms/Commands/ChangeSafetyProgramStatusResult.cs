// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Result payload for ChangeSafetyProgramStatusCommand.
// SchemaVersion: 1

namespace PlantSafety.Application.SafetyPrograms.Commands;

/// <summary>Result of a successful <see cref="ChangeSafetyProgramStatusCommand"/> execution.</summary>
/// <param name="SafetyProgramId">Identity of the affected SafetyProgram aggregate.</param>
/// <param name="Status">The program's new lifecycle status.</param>
public sealed record ChangeSafetyProgramStatusResult(
    Guid SafetyProgramId,
    string Status);
