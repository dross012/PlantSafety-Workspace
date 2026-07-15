// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTOs mirroring the Checklists read-model
//   endpoint (GET v1/operational/checklists). Deserialized directly into
//   the inner Data shape of the ApiResponse&lt;T&gt; envelope, matching the
//   established sibling convention (see
//   Modules/PlanningProjects/Models/PlanningProjectsListResponseDto.cs).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.Checklist.Models;

/// <summary>A single Checklist template/instance record (GET v1/operational/checklists).</summary>
public sealed class ChecklistItemDto
{
    /// <summary>Checklist record identifier.</summary>
    public Guid ChecklistId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Checklist name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Checklist category grouping.</summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>Current status of this record.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>UTC timestamp when this record was last actioned.</summary>
    public DateTime CompletedAtUtc { get; init; }

    /// <summary>Identity of the user who last actioned this record.</summary>
    public Guid CompletedBy { get; init; }
}

/// <summary>Response payload for the Checklists read-model list endpoint.</summary>
public sealed class ChecklistsResponseDto
{
    /// <summary>Gets the tenant the data belongs to.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site the data is scoped to, or null for tenant-wide results.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the UTC timestamp when this read-model snapshot was generated.</summary>
    public DateTime GeneratedAt { get; init; }

    /// <summary>Gets the matching checklist items. Never null; empty when no results.</summary>
    public IReadOnlyList<ChecklistItemDto> Items { get; init; } = [];
}
