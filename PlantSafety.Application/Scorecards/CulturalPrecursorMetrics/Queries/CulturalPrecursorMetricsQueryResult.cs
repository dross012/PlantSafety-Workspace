// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetCulturalPrecursorMetricsQuery.
// SchemaVersion: 1

using PlantSafety.Application.Scorecards.CulturalPrecursorMetrics.Models;

namespace PlantSafety.Application.Scorecards.CulturalPrecursorMetrics.Queries;

/// <summary>Result of <see cref="GetCulturalPrecursorMetricsQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Cultural & Precursor Metrics (Near-Miss, Behavioral Observations) items.</param>
public sealed record CulturalPrecursorMetricsQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<CulturalPrecursorMetricsItemDto> Items);