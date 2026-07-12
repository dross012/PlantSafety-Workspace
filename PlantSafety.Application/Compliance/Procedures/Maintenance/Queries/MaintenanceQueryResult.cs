// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetMaintenanceQuery.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Procedures.Maintenance.Models;

namespace PlantSafety.Application.Compliance.Procedures.Maintenance.Queries;

/// <summary>Result of <see cref="GetMaintenanceQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Maintenance items.</param>
public sealed record MaintenanceQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<MaintenanceItemDto> Items);
