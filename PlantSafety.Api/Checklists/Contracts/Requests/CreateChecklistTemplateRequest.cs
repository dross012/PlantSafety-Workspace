// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Request body for creating a new ChecklistTemplate over HTTP.

namespace PlantSafety.Api.Checklists.Contracts.Requests;

/// <summary>Request body for <c>POST /api/v1/checklists/templates</c>.</summary>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="TemplateTitle">Human-readable checklist template title.</param>
/// <param name="Category">ChecklistCategory code.</param>
/// <param name="Description">Narrative description of what this checklist template covers.</param>
/// <param name="ItemCount">Number of checklist items defined on this template.</param>
/// <param name="IsMandatory">Whether completion of this checklist is mandatory.</param>
/// <param name="RegulatoryReference">Optional regulatory citation this checklist supports.</param>
public sealed record CreateChecklistTemplateRequest(
    Guid? SiteId,
    string TemplateTitle,
    string Category,
    string Description,
    int ItemCount,
    bool IsMandatory,
    string? RegulatoryReference);
