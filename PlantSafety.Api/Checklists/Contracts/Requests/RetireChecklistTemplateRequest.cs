// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Request body for retiring a ChecklistTemplate over HTTP.

namespace PlantSafety.Api.Checklists.Contracts.Requests;

/// <summary>Request body for <c>POST /api/v1/checklists/templates/{checklistTemplateId}/retire</c>.</summary>
/// <param name="Reason">Narrative reason the template is being retired.</param>
public sealed record RetireChecklistTemplateRequest(string Reason);
