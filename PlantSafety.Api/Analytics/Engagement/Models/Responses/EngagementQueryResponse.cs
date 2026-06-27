// ============================================================================
// File: EngagementQueryResponse.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: Internal Presentation-layer response model for Engagement analytics, before projection to the external Contracts/Responses DTO.
// ============================================================================
//
// Onboarding Notes:
// - Populated from the Application-layer result and then projected to EngagementResponse in the controller or MappingProfile.
//
// Rationale:
// - Provides a stable internal model that absorbs Application-layer shape changes without immediately breaking the external contract.
//
// Future Extension Seams:
// - Add computed Presentation-layer fields here (e.g. FormattedValue, LocalisedLabel) that enrich the Application result before serialisation.
//
// ============================================================================

namespace PlantSafety.Api.Analytics.Engagement.Models.Responses;

/// <summary>Internal Presentation-layer response model for Engagement analytics queries.</summary>
/// <param name="TenantId">Owning tenant.</param>
/// <param name="SiteId">Scoped site, or null for tenant-wide data.</param>
/// <param name="GeneratedAt">Snapshot generation timestamp (UTC).</param>
/// <param name="Data">Result data points from the Application layer.</param>
public sealed record EngagementQueryResponse(
    Guid     TenantId,
    Guid?    SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<EngagementQueryDataPoint> Data);

/// <summary>Single data point in the internal Engagement query response.</summary>
/// <param name="Label">Metric label.</param>
/// <param name="Value">Numeric value.</param>
/// <param name="Trend">Trend direction string.</param>
public sealed record EngagementQueryDataPoint(string Label, decimal Value, string? Trend);