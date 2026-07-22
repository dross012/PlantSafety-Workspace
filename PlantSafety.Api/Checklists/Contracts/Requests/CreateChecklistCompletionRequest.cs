// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Request body for creating a new ChecklistCompletion over HTTP.

namespace PlantSafety.Api.Checklists.Contracts.Requests;

/// <summary>Request body for <c>POST /api/v1/checklists/completions</c>.</summary>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="ChecklistTemplateId">Template this completion is executing against.</param>
/// <param name="ItemsTotal">Total number of items expected to be completed.</param>
/// <param name="ItemsPassed">Number of items passed so far.</param>
/// <param name="ItemsFailed">Number of items failed so far.</param>
/// <param name="Outcome">ChecklistCompletionOutcome code.</param>
public sealed record CreateChecklistCompletionRequest(
    Guid? SiteId,
    Guid ChecklistTemplateId,
    int ItemsTotal,
    int ItemsPassed,
    int ItemsFailed,
    string Outcome);
