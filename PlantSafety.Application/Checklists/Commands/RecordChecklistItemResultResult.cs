// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Result payload for RecordChecklistItemResultCommand.
// SchemaVersion: 1

namespace PlantSafety.Application.Checklists.Commands;

/// <summary>Result of a successful <see cref="RecordChecklistItemResultCommand"/> execution.</summary>
/// <param name="ChecklistCompletionId">Identity of the ChecklistCompletion the result was recorded against.</param>
/// <param name="ItemResultId">Identity of the newly recorded item result.</param>
/// <param name="ItemsPassed">Running count of passed items.</param>
/// <param name="ItemsFailed">Running count of failed items.</param>
public sealed record RecordChecklistItemResultResult(
    Guid ChecklistCompletionId,
    Guid ItemResultId,
    int ItemsPassed,
    int ItemsFailed);
