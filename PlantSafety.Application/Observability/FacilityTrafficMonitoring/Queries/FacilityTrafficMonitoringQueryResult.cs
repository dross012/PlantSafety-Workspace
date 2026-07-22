// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetFacilityTrafficMonitoringQuery.
// SchemaVersion: 1

using PlantSafety.Application.Observability.FacilityTrafficMonitoring.Models;

namespace PlantSafety.Application.Observability.FacilityTrafficMonitoring.Queries;

/// <summary>Result of <see cref="GetFacilityTrafficMonitoringQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching FacilityTrafficMonitoring items.</param>
public sealed record FacilityTrafficMonitoringQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<FacilityTrafficMonitoringItemDto> Items);