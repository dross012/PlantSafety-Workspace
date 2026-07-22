// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Result payload for PublishChecklistTemplateCommand.
// SchemaVersion: 1

namespace PlantSafety.Application.Checklists.Commands;

/// <summary>Result of a successful <see cref="PublishChecklistTemplateCommand"/> execution.</summary>
/// <param name="ChecklistTemplateId">Identity of the published ChecklistTemplate.</param>
/// <param name="Status">Lifecycle status after publication.</param>
/// <param name="ItemCount">Total line items on the published template.</param>
public sealed record PublishChecklistTemplateResult(
    Guid ChecklistTemplateId,
    string Status,
    int ItemCount);
