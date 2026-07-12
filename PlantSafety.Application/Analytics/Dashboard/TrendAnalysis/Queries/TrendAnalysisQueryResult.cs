// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetTrendAnalysisQuery.
// SchemaVersion: 1

using PlantSafety.Application.Analytics.Dashboard.TrendAnalysis.Models;

namespace PlantSafety.Application.Analytics.Dashboard.TrendAnalysis.Queries;

/// <summary>Result of <see cref="GetTrendAnalysisQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching TrendAnalysis items.</param>
public sealed record TrendAnalysisQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<TrendAnalysisItemDto> Items);
