// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Result payload for RetireChecklistTemplateCommand.
// SchemaVersion: 1

namespace PlantSafety.Application.Checklists.Commands;

/// <summary>Result of a successful <see cref="RetireChecklistTemplateCommand"/> execution.</summary>
/// <param name="ChecklistTemplateId">Identity of the retired ChecklistTemplate.</param>
/// <param name="Status">Lifecycle status after retirement.</param>
public sealed record RetireChecklistTemplateResult(
    Guid ChecklistTemplateId,
    string Status);
