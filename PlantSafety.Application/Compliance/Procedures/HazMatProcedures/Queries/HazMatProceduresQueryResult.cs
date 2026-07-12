// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetHazMatProceduresQuery.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Procedures.HazMatProcedures.Models;

namespace PlantSafety.Application.Compliance.Procedures.HazMatProcedures.Queries;

/// <summary>Result of <see cref="GetHazMatProceduresQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching HazMatProcedures items.</param>
public sealed record HazMatProceduresQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<HazMatProceduresItemDto> Items);
