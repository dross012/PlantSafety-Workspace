// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Request body for adding an item to a ChecklistTemplate over HTTP.

namespace PlantSafety.Api.Checklists.Contracts.Requests;

/// <summary>Request body for <c>POST /api/v1/checklists/templates/{checklistTemplateId}/items</c>.</summary>
/// <param name="Description">Narrative description of the checklist item.</param>
/// <param name="IsCritical">Whether a failure of this item is a critical (stop-work) failure.</param>
/// <param name="RegulatoryReference">Optional regulatory citation for this item.</param>
public sealed record AddChecklistTemplateItemRequest(
    string Description,
    bool IsCritical,
    string? RegulatoryReference);
