// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Generic client-side response envelope shared by every
//   Analytics section read-model endpoint (Culture/Dashboard/Engagement/
//   Predictive/Reporting/Schedules/ScoreCards/TrendAnalysis). Mirrors the
//   {TenantId, SiteId, GeneratedAt, Items} shape returned by every
//   PlantSafety.Api.Analytics.* V1 controller (e.g. ExecutiveResponse,
//   BehaviorResponse, DepartmentResponse) — same pattern already used by
//   PlantSafety.UI.Modules.Admin.Models.AdminListResponseDto&lt;T&gt; for the
//   CrossDomain/Integrations read-model endpoints. Declared once here and
//   reused across all 8 Analytics modules owned by this batch to avoid
//   duplicating an identical envelope shape 8 times.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.DashboardAnalytics.Models;

/// <summary>Generic list-response envelope shared by every Analytics section read-model endpoint.</summary>
/// <typeparam name="TItem">The item DTO type for this section.</typeparam>
public sealed class AnalyticsSectionResponseDto<TItem>
{
    /// <summary>Gets the tenant the data belongs to.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site the data is scoped to, or null for tenant-wide results.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the UTC timestamp when this read-model snapshot was generated.</summary>
    public DateTime GeneratedAt { get; init; }

    /// <summary>Gets the result items. Never null; empty when no results.</summary>
    public IReadOnlyList<TItem> Items { get; init; } = [];
}
