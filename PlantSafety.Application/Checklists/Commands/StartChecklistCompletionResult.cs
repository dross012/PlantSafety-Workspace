// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Result payload for StartChecklistCompletionCommand.
// SchemaVersion: 1

namespace PlantSafety.Application.Checklists.Commands;

/// <summary>Result of a successful <see cref="StartChecklistCompletionCommand"/> execution.</summary>
/// <param name="ChecklistCompletionId">Identity of the started ChecklistCompletion.</param>
/// <param name="Status">Lifecycle status after starting.</param>
public sealed record StartChecklistCompletionResult(
    Guid ChecklistCompletionId,
    string Status);
