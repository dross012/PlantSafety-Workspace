// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetTrendModelingQuery.
// SchemaVersion: 1

using PlantSafety.Application.Analytics.Culture.TrendModeling.Models;

namespace PlantSafety.Application.Analytics.Culture.TrendModeling.Queries;

/// <summary>Result of <see cref="GetTrendModelingQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching TrendModeling items.</param>
public sealed record TrendModelingQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<TrendModelingItemDto> Items);
