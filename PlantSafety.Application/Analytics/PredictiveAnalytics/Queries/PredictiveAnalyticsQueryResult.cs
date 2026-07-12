// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetPredictiveAnalyticsQuery.
// SchemaVersion: 1

using PlantSafety.Application.Analytics.PredictiveAnalytics.Models;

namespace PlantSafety.Application.Analytics.PredictiveAnalytics.Queries;

/// <summary>Result of <see cref="GetPredictiveAnalyticsQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching PredictiveAnalytics items.</param>
public sealed record PredictiveAnalyticsQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<PredictiveAnalyticsItemDto> Items);
