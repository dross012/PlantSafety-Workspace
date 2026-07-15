// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side item DTO for the Reporting Analytics module.
//   Mirrors PlantSafety.Application.Analytics.Reporting.Models.ReportingItemDto
//   field-for-field so ReportingAnalyticsApiClient can deserialize the real
//   backend response from api/v1/analytics/reporting directly. Display-only
//   — no domain logic.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;

namespace PlantSafety.UI.Modules.ReportingAnalytics.Models;

/// <summary>A single generated analytics report entry (mirrors <c>ReportingItemDto</c>).</summary>
public sealed class ReportEntryDto
{
    /// <summary>Gets the report item identifier.</summary>
    public Guid ReportItemId { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the report name.</summary>
    public string ReportName { get; init; } = string.Empty;

    /// <summary>Gets the report category (e.g., Compliance, Safety, Operational).</summary>
    public string ReportCategory { get; init; } = string.Empty;

    /// <summary>Gets the current status (e.g., Queued, Generated, Failed).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp when the report was generated.</summary>
    public DateTime GeneratedAtUtc { get; init; }

    /// <summary>Gets the identity of the user or process that generated the report.</summary>
    public Guid GeneratedBy { get; init; }
}

/// <summary>A client-side aggregate row for the By Category breakdown view (computed presentation-only, no domain logic).</summary>
/// <param name="Category">The report category label.</param>
/// <param name="Count">The number of reports in this category.</param>
/// <param name="FailedCount">The number of reports in this category with a Failed status.</param>
public sealed record ReportCategoryBreakdown(string Category, int Count, int FailedCount);
