// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetLotoProceduresQuery.
// SchemaVersion: 1

using PlantSafety.Application.SafetyProcedures.LotoProcedures.Models;

namespace PlantSafety.Application.SafetyProcedures.LotoProcedures.Queries;

/// <summary>Result of <see cref="GetLotoProceduresQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching LotoProcedures items.</param>
public sealed record LotoProceduresQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<LotoProceduresItemDto> Items);