// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetLotoProgramsQuery.
// SchemaVersion: 1

using PlantSafety.Application.EmployeeSafety.LotoPrograms.Models;

namespace PlantSafety.Application.EmployeeSafety.LotoPrograms.Queries;

/// <summary>Result of <see cref="GetLotoProgramsQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching LotoPrograms items.</param>
public sealed record LotoProgramsQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<LotoProgramsItemDto> Items);