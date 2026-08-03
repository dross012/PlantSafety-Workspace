// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Result payload for CreateSafetyProgramCommand.
// SchemaVersion: 1

namespace PlantSafety.Application.SafetyPrograms.Commands;

/// <summary>Result of a successful <see cref="CreateSafetyProgramCommand"/> execution.</summary>
/// <param name="SafetyProgramId">Identity of the newly created SafetyProgram aggregate.</param>
/// <param name="Status">Lifecycle status of the newly created record.</param>
public sealed record CreateSafetyProgramResult(
    Guid SafetyProgramId,
    string Status);
