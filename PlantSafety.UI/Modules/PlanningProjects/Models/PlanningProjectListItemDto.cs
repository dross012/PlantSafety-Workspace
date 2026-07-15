// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTO mirroring the server-side
//   PlanningProjectItemDto returned by GET /api/v1/planning-projects. Contains
//   only the fields exposed by that read-model list endpoint — not the full
//   Safety Project detail (see SafetyProjectDto for the detail-page shape).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;

namespace PlantSafety.UI.Modules.PlanningProjects.Models;

/// <summary>
/// Lightweight planning project list item as returned by the PlanningProjects
/// read-model list endpoint (<c>v1/planning-projects</c>).
/// </summary>
public sealed class PlanningProjectListItemDto
{
    /// <summary>Gets the project identifier.</summary>
    public Guid ProjectId { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the project name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets the current lifecycle status code.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp when the project is targeted to complete.</summary>
    public DateTime TargetCompletionUtc { get; init; }

    /// <summary>Gets the identity of the project owner.</summary>
    public Guid OwnerId { get; init; }
}
