// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for CreateScorecardCommand.
// SchemaVersion: 1

namespace PlantSafety.Application.Scorecards.Commands;

/// <summary>Result of a successful <see cref="CreateScorecardCommand"/> execution.</summary>
/// <param name="ScorecardId">Identity of the newly created Scorecard aggregate.</param>
/// <param name="Status">Lifecycle status of the newly created record.</param>
public sealed record CreateScorecardResult(
    Guid ScorecardId,
    string Status);