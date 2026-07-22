// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Result payload for CompleteChecklistCompletionCommand.
// SchemaVersion: 1

namespace PlantSafety.Application.Checklists.Commands;

/// <summary>Result of a successful <see cref="CompleteChecklistCompletionCommand"/> execution.</summary>
/// <param name="ChecklistCompletionId">Identity of the completed ChecklistCompletion.</param>
/// <param name="Status">Lifecycle status after completion.</param>
/// <param name="Outcome">Overall outcome: Pass, Fail, or PartialWithFindings.</param>
/// <param name="ItemsTotal">Total items evaluated.</param>
/// <param name="ItemsPassed">Items that passed.</param>
/// <param name="ItemsFailed">Items that failed.</param>
public sealed record CompleteChecklistCompletionResult(
    Guid ChecklistCompletionId,
    string Status,
    string Outcome,
    int ItemsTotal,
    int ItemsPassed,
    int ItemsFailed);
