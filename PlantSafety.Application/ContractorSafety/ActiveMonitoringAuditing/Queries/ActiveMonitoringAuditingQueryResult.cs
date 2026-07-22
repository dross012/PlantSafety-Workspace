// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetActiveMonitoringAuditingQuery.
// SchemaVersion: 1

using PlantSafety.Application.ContractorSafety.ActiveMonitoringAuditing.Models;

namespace PlantSafety.Application.ContractorSafety.ActiveMonitoringAuditing.Queries;

/// <summary>Result of <see cref="GetActiveMonitoringAuditingQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching ActiveMonitoringAuditing items.</param>
public sealed record ActiveMonitoringAuditingQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<ActiveMonitoringAuditingItemDto> Items);