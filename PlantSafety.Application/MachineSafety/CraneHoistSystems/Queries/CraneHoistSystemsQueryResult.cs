// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Result payload for GetCraneHoistSystemsQuery.
// SchemaVersion: 1

using PlantSafety.Application.MachineSafety.CraneHoistSystems.Models;

namespace PlantSafety.Application.MachineSafety.CraneHoistSystems.Queries;

/// <summary>Result of <see cref="GetCraneHoistSystemsQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching CraneHoistSystems items.</param>
public sealed record CraneHoistSystemsQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<CraneHoistSystemsItemDto> Items);
