// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for CreateChecklistCompletionCommand.
// SchemaVersion: 1

namespace PlantSafety.Application.Checklists.Commands;

/// <summary>Result of a successful <see cref="CreateChecklistCompletionCommand"/> execution.</summary>
/// <param name="ChecklistCompletionId">Identity of the newly created ChecklistCompletion aggregate.</param>
/// <param name="Status">Lifecycle status of the newly created record.</param>
public sealed record CreateChecklistCompletionResult(
    Guid ChecklistCompletionId,
    string Status);