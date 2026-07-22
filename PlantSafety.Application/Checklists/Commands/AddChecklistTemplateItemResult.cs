// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Result payload for AddChecklistTemplateItemCommand.
// SchemaVersion: 1

namespace PlantSafety.Application.Checklists.Commands;

/// <summary>Result of a successful <see cref="AddChecklistTemplateItemCommand"/> execution.</summary>
/// <param name="ChecklistTemplateId">Identity of the parent ChecklistTemplate.</param>
/// <param name="ItemId">Identity of the newly added line item.</param>
/// <param name="Sequence">Assigned display/execution sequence of the item.</param>
public sealed record AddChecklistTemplateItemResult(
    Guid ChecklistTemplateId,
    Guid ItemId,
    int Sequence);
