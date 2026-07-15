// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTOs for the Engagement Analytics module.
//   Mirror PlantSafety.Api.Analytics.Engagement.Contracts.Responses.
//   EngagementResponse/EngagementDataPoint field-for-field so
//   EngagementAnalyticsApiClient can deserialize the real backend response
//   from api/v1/analytics/engagement directly. Note this endpoint uses a
//   "Data" property (not "Items") — a simpler (Label, Value, Trend) shape
//   than the other Analytics sections, so it is not expressed via the
//   generic AnalyticsSectionResponseDto&lt;TItem&gt;. Display-only — no
//   domain logic.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.EngagementAnalytics.Models;

/// <summary>Response payload for Engagement analytics queries (mirrors <c>EngagementResponse</c>).</summary>
public sealed class EngagementResponseDto
{
    /// <summary>Gets the tenant the data belongs to.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site the data is scoped to, or null for tenant-wide aggregates.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the UTC timestamp when this read-model snapshot was generated.</summary>
    public DateTime GeneratedAt { get; init; }

    /// <summary>Gets the Engagement analytics data points. Never null; empty when no results.</summary>
    public IReadOnlyList<EngagementDataPointDto> Data { get; init; } = [];
}

/// <summary>A single data point within an Engagement analytics result set (mirrors <c>EngagementDataPoint</c>).</summary>
public sealed class EngagementDataPointDto
{
    /// <summary>Gets the human-readable label (department, date bucket, category, etc.).</summary>
    public string Label { get; init; } = string.Empty;

    /// <summary>Gets the primary numeric metric value.</summary>
    public decimal Value { get; init; }

    /// <summary>Gets the trend direction: Improving, Stable, or Declining. May be null.</summary>
    public string? Trend { get; init; }
}
