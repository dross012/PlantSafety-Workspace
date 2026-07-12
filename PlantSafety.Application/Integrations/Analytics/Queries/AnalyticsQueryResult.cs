// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Result payload for GetAnalyticsQuery.
// SchemaVersion: 1

using PlantSafety.Application.Integrations.Analytics.Models;

namespace PlantSafety.Application.Integrations.Analytics.Queries;

/// <summary>Result of <see cref="GetAnalyticsQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Integrations.Analytics items.</param>
public sealed record AnalyticsQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<AnalyticsItemDto> Items);
