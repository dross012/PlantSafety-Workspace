// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Request body for flagging a failed checklist item over HTTP.

namespace PlantSafety.Api.Checklists.Contracts.Requests;

/// <summary>Request body for <c>POST /api/v1/checklists/completions/{checklistCompletionId}/items/{checklistTemplateItemId}/flag-failed</c>.</summary>
/// <param name="Description">Narrative description of the failure.</param>
public sealed record FlagFailedChecklistItemRequest(string Description);
