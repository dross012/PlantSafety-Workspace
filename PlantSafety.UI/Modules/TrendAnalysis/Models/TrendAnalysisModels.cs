// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side item DTO for the Trend Analysis module.
//   Mirrors the top-level PlantSafety.Application.Analytics.TrendAnalysis.
//   Models.TrendAnalysisItemDto field-for-field so TrendAnalysisApiClient
//   can deserialize the real backend response from
//   api/v1/analytics/trendanalysis directly (Volume A1 §Page 2 — Trend
//   Explorer: multi-series chart + Category/Count/Trend breakdown table).
//   Display-only — no domain logic (trend direction is always server-computed,
//   never derived client-side, per UI001).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;

namespace PlantSafety.UI.Modules.TrendAnalysis.Models;

/// <summary>A single analytics trend data point (mirrors the top-level <c>TrendAnalysisItemDto</c>).</summary>
public sealed class TrendAnalysisPointDto
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

    /// <summary>Gets the current status (e.g., Provisional, Final).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp when this point was calculated.</summary>
    public DateTime CalculatedAtUtc { get; init; }
}

/// <summary>A client-side aggregate breakdown row grouped by metric (Category/Count/Trend, computed presentation-only, no domain logic).</summary>
/// <param name="MetricName">The metric name (breakdown category).</param>
/// <param name="Count">The number of trend points recorded for this metric.</param>
/// <param name="LatestValue">The most recently calculated value for this metric.</param>
/// <param name="LatestTrendDirection">The most recently calculated trend direction for this metric.</param>
public sealed record TrendBreakdownRow(string MetricName, int Count, decimal LatestValue, string LatestTrendDirection);
