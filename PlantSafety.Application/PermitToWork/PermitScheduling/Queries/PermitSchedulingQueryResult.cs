// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetPermitSchedulingQuery.
// SchemaVersion: 1

using PlantSafety.Application.PermitToWork.PermitScheduling.Models;

namespace PlantSafety.Application.PermitToWork.PermitScheduling.Queries;

/// <summary>Result of <see cref="GetPermitSchedulingQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching PermitScheduling items.</param>
public sealed record PermitSchedulingQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<PermitSchedulingItemDto> Items);