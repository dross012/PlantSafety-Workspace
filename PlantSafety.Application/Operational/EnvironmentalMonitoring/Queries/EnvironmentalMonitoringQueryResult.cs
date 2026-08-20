// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-20
// FILE DESCRIPTION: Result payload for GetEnvironmentalMonitoringQuery.
// SchemaVersion: 1

using PlantSafety.Application.Operational.EnvironmentalMonitoring.Models;

namespace PlantSafety.Application.Operational.EnvironmentalMonitoring.Queries;

/// <summary>Result of <see cref="GetEnvironmentalMonitoringQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching EnvironmentalMonitoring items.</param>
public sealed record EnvironmentalMonitoringQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<EnvironmentalMonitoringItemDto> Items);