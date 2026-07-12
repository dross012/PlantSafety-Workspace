// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetLockoutTagoutProcedureQuery.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Procedures.LockoutTagoutProcedure.Models;

namespace PlantSafety.Application.Compliance.Procedures.LockoutTagoutProcedure.Queries;

/// <summary>Result of <see cref="GetLockoutTagoutProcedureQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching LockoutTagoutProcedure items.</param>
public sealed record LockoutTagoutProcedureQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<LockoutTagoutProcedureItemDto> Items);
