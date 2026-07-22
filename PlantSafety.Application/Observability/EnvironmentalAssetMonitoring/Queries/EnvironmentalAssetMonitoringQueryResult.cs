// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetEnvironmentalAssetMonitoringQuery.
// SchemaVersion: 1

using PlantSafety.Application.Observability.EnvironmentalAssetMonitoring.Models;

namespace PlantSafety.Application.Observability.EnvironmentalAssetMonitoring.Queries;

/// <summary>Result of <see cref="GetEnvironmentalAssetMonitoringQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching EnvironmentalAssetMonitoring items.</param>
public sealed record EnvironmentalAssetMonitoringQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<EnvironmentalAssetMonitoringItemDto> Items);