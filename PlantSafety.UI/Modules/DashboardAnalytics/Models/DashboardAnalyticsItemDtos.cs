// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side item DTOs for the Dashboard Analytics module.
//   Mirror the PlantSafety.Application.Analytics.Dashboard.* read-model item
//   DTOs field-for-field (ExecutiveItemDto/KPISnapshotsItemDto/ReadModelItemDto/
//   TrendAnalysisItemDto) so DashboardAnalyticsApiClient can deserialize real
//   backend responses from api/v1/analytics/dashboard/{executive,kpisnapshots,
//   readmodel,trendanalysis} directly. Display-only — no domain logic.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;

namespace PlantSafety.UI.Modules.DashboardAnalytics.Models;

/// <summary>A single executive-level summary metric (mirrors <c>ExecutiveItemDto</c>).</summary>
public sealed class ExecutiveMetricDto
{
    /// <summary>Gets the summary metric identifier.</summary>
    public Guid SummaryId { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the metric name (e.g., TRIR, Open Corrective Actions).</summary>
    public string MetricName { get; init; } = string.Empty;

    /// <summary>Gets the current metric value.</summary>
    public decimal MetricValue { get; init; }

    /// <summary>Gets the target value for the metric, if defined.</summary>
    public decimal? TargetValue { get; init; }

    /// <summary>Gets the current status relative to target (e.g., OnTrack, AtRisk, Breached).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp when this snapshot was captured.</summary>
    public DateTime SnapshotAtUtc { get; init; }
}

/// <summary>A single point-in-time KPI snapshot (mirrors <c>KPISnapshotsItemDto</c>).</summary>
public sealed class KpiSnapshotDto
{
    /// <summary>Gets the snapshot identifier.</summary>
    public Guid SnapshotId { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the KPI name (e.g., Near Miss Rate, Audit Completion %).</summary>
    public string KpiName { get; init; } = string.Empty;

    /// <summary>Gets the KPI value at the time of the snapshot.</summary>
    public decimal KpiValue { get; init; }

    /// <summary>Gets the unit of measure (e.g., Percent, Count, Rate).</summary>
    public string UnitOfMeasure { get; init; } = string.Empty;

    /// <summary>Gets the current status (e.g., Provisional, Final).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp when the snapshot was captured.</summary>
    public DateTime SnapshotAtUtc { get; init; }
}

/// <summary>A single generic dashboard widget row (mirrors <c>ReadModelItemDto</c>).</summary>
public sealed class DashboardWidgetRowDto
{
    /// <summary>Gets the row identifier.</summary>
    public Guid RowId { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the widget name this row feeds.</summary>
    public string WidgetName { get; init; } = string.Empty;

    /// <summary>Gets the rendered value for the widget row.</summary>
    public string WidgetValue { get; init; } = string.Empty;

    /// <summary>Gets the ordinal display position within the dashboard.</summary>
    public int DisplayOrder { get; init; }

    /// <summary>Gets the current status (e.g., Active, Stale).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp when this row was last refreshed.</summary>
    public DateTime RefreshedAtUtc { get; init; }
}

/// <summary>A single dashboard trend data point (mirrors the Dashboard-scoped <c>TrendAnalysisItemDto</c>).</summary>
public sealed class DashboardTrendPointDto
{
    /// <summary>Gets the trend point identifier.</summary>
    public Guid TrendPointId { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the metric name being trended.</summary>
    public string MetricName { get; init; } = string.Empty;

    /// <summary>Gets the metric value at this point.</summary>
    public decimal MetricValue { get; init; }

    /// <summary>Gets the trend direction versus the prior period (e.g., Improving, Stable, Declining).</summary>
    public string TrendDirection { get; init; } = string.Empty;

    /// <summary>Gets the current status of the data point (e.g., Provisional, Final).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp when this point was calculated.</summary>
    public DateTime CalculatedAtUtc { get; init; }
}
