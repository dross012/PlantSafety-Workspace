// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTO mirroring PlantSafety.Api's
//   PlanningProjectsResponse envelope returned inside ApiResponse&lt;T&gt;
//   by GET /api/v1/planning-projects.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.PlanningProjects.Models;

/// <summary>Response payload for the PlanningProjects read-model list endpoint.</summary>
public sealed class PlanningProjectsListResponseDto
{
    /// <summary>Gets the tenant the data belongs to.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site the data is scoped to, or null for tenant-wide results.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the UTC timestamp when this read-model snapshot was generated.</summary>
    public DateTime GeneratedAt { get; init; }

    /// <summary>Gets the matching planning project items. Never null; empty when no results.</summary>
    public IReadOnlyList<PlanningProjectListItemDto> Items { get; init; } = [];
}
