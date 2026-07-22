// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for CreateChecklistTemplateCommand.
// SchemaVersion: 1

namespace PlantSafety.Application.Checklists.Commands;

/// <summary>Result of a successful <see cref="CreateChecklistTemplateCommand"/> execution.</summary>
/// <param name="ChecklistTemplateId">Identity of the newly created ChecklistTemplate aggregate.</param>
/// <param name="Status">Lifecycle status of the newly created record.</param>
public sealed record CreateChecklistTemplateResult(
    Guid ChecklistTemplateId,
    string Status);