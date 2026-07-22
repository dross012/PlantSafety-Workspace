// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetEnvironmentalHealthMonitoringQuery.
// SchemaVersion: 1

using PlantSafety.Application.EmployeeSafety.EnvironmentalHealthMonitoring.Models;

namespace PlantSafety.Application.EmployeeSafety.EnvironmentalHealthMonitoring.Queries;

/// <summary>Result of <see cref="GetEnvironmentalHealthMonitoringQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching EnvironmentalHealthMonitoring items.</param>
public sealed record EnvironmentalHealthMonitoringQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<EnvironmentalHealthMonitoringItemDto> Items);