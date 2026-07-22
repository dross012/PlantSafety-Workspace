// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Request body for recording an item result within a ChecklistCompletion over HTTP.

namespace PlantSafety.Api.Checklists.Contracts.Requests;

/// <summary>Request body for <c>POST /api/v1/checklists/completions/{checklistCompletionId}/items/{checklistTemplateItemId}/result</c>.</summary>
/// <param name="Passed">Whether the checklist item passed inspection.</param>
/// <param name="Notes">Optional inspector notes.</param>
public sealed record RecordChecklistItemResultRequest(
    bool Passed,
    string? Notes);
