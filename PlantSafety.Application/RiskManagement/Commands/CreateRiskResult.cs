// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for CreateRiskCommand.
// SchemaVersion: 1

namespace PlantSafety.Application.RiskManagement.Commands;

/// <summary>Result of a successful <see cref="CreateRiskCommand"/> execution.</summary>
/// <param name="RiskId">Identity of the newly created Risk aggregate.</param>
/// <param name="Status">Lifecycle status of the newly created record.</param>
public sealed record CreateRiskResult(
    Guid RiskId,
    string Status);