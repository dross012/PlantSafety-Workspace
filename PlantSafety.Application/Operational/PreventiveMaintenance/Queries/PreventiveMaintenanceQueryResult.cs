// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetPreventiveMaintenanceQuery.
// SchemaVersion: 1

using PlantSafety.Application.Operational.PreventiveMaintenance.Models;

namespace PlantSafety.Application.Operational.PreventiveMaintenance.Queries;

/// <summary>Result of <see cref="GetPreventiveMaintenanceQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching PreventiveMaintenance items.</param>
public sealed record PreventiveMaintenanceQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<PreventiveMaintenanceItemDto> Items);
