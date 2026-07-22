// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Result payload for FlagFailedChecklistItemCommand.
// SchemaVersion: 1

namespace PlantSafety.Application.Checklists.Commands;

/// <summary>Result of a successful <see cref="FlagFailedChecklistItemCommand"/> execution.</summary>
/// <param name="ChecklistCompletionId">Identity of the ChecklistCompletion the flag was recorded against.</param>
/// <param name="ItemResultId">Identity of the flagged item result.</param>
/// <param name="CriticalFailureCount">Running count of critical/stop-work failures on this completion.</param>
public sealed record FlagFailedChecklistItemResult(
    Guid ChecklistCompletionId,
    Guid ItemResultId,
    int CriticalFailureCount);
