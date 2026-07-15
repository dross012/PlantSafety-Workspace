// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTOs mirroring the three Compliance
//   dashboard read-model endpoints (Compliance/RegulatoryRequirements,
//   Compliance/Obligations, Compliance/Compliance/Tracking). Each endpoint
//   returns an ApiResponse&lt;T&gt;-wrapped envelope whose Data payload has
//   this shape (TenantId/SiteId/GeneratedAt/Items) — the outer ApiResponse
//   envelope is intentionally not modeled here, matching the established
//   sibling pattern (Modules/PlanningProjects/Models/PlanningProjectsListResponseDto)
//   of deserializing directly into the inner Data shape.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.Compliance.Models;

/// <summary>A single applicable regulatory requirement (GET v1/compliance/compliance/regulatoryrequirements).</summary>
public sealed class RequirementItemDto
{
    /// <summary>Requirement identifier.</summary>
    public Guid RequirementId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Short title describing the requirement.</summary>
    public string RequirementTitle { get; init; } = string.Empty;

    /// <summary>Jurisdiction the requirement is enforced under (e.g., Federal, State, Local).</summary>
    public string Jurisdiction { get; init; } = string.Empty;

    /// <summary>Current requirement status (e.g., Applicable/Superseded/UnderReview).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>UTC timestamp when the requirement became effective.</summary>
    public DateTime EffectiveAtUtc { get; init; }

    /// <summary>Identity of the user who owns tracking of the requirement.</summary>
    public Guid OwnerId { get; init; }
}

/// <summary>Response payload for the Regulatory Requirements read-model list endpoint.</summary>
public sealed class RequirementsResponseDto
{
    /// <summary>Gets the tenant the data belongs to.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site the data is scoped to, or null for tenant-wide results.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the UTC timestamp when this read-model snapshot was generated.</summary>
    public DateTime GeneratedAt { get; init; }

    /// <summary>Gets the matching requirement items. Never null; empty when no results.</summary>
    public IReadOnlyList<RequirementItemDto> Items { get; init; } = [];
}

/// <summary>A single compliance obligation owed to a regulatory body (GET v1/compliance/compliance/obligations).</summary>
public sealed class ObligationItemDto
{
    /// <summary>Obligation identifier.</summary>
    public Guid ObligationId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Short title describing the obligation.</summary>
    public string ObligationTitle { get; init; } = string.Empty;

    /// <summary>Regulatory body the obligation is owed to.</summary>
    public string RegulatoryBody { get; init; } = string.Empty;

    /// <summary>Current obligation status (e.g., Open/Fulfilled/Overdue).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>UTC timestamp when the obligation is due.</summary>
    public DateTime DueAtUtc { get; init; }

    /// <summary>Identity of the user who owns the obligation.</summary>
    public Guid OwnerId { get; init; }
}

/// <summary>Response payload for the Obligations read-model list endpoint.</summary>
public sealed class ObligationsResponseDto
{
    /// <summary>Gets the tenant the data belongs to.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site the data is scoped to, or null for tenant-wide results.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the UTC timestamp when this read-model snapshot was generated.</summary>
    public DateTime GeneratedAt { get; init; }

    /// <summary>Gets the matching obligation items. Never null; empty when no results.</summary>
    public IReadOnlyList<ObligationItemDto> Items { get; init; } = [];
}

/// <summary>A single compliance tracking item (GET v1/compliance/compliance/tracking).</summary>
public sealed class TrackingItemDto
{
    /// <summary>Tracking item identifier.</summary>
    public Guid TrackingItemId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Description of the tracked item.</summary>
    public string ItemDescription { get; init; } = string.Empty;

    /// <summary>Category of the tracked item (e.g., Permit Renewal, Audit Action, Training).</summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>Current tracking status (e.g., OnTrack/AtRisk/Overdue/Closed).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>UTC timestamp when the tracking item was last updated.</summary>
    public DateTime LastUpdatedAtUtc { get; init; }

    /// <summary>Identity of the user who owns the tracked item.</summary>
    public Guid OwnerId { get; init; }
}

/// <summary>Response payload for the Compliance Tracking read-model list endpoint.</summary>
public sealed class TrackingResponseDto
{
    /// <summary>Gets the tenant the data belongs to.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site the data is scoped to, or null for tenant-wide results.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the UTC timestamp when this read-model snapshot was generated.</summary>
    public DateTime GeneratedAt { get; init; }

    /// <summary>Gets the matching tracking items. Never null; empty when no results.</summary>
    public IReadOnlyList<TrackingItemDto> Items { get; init; } = [];
}
