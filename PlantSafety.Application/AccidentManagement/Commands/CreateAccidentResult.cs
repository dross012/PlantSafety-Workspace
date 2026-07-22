// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for CreateAccidentCommand.
// SchemaVersion: 1

namespace PlantSafety.Application.AccidentManagement.Commands;

/// <summary>Result of a successful <see cref="CreateAccidentCommand"/> execution.</summary>
/// <param name="AccidentId">Identity of the newly created Accident aggregate.</param>
/// <param name="Status">Lifecycle status of the newly created record.</param>
public sealed record CreateAccidentResult(
    Guid AccidentId,
    string Status);