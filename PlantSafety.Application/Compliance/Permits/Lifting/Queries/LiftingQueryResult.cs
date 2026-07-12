// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetLiftingQuery.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Permits.Lifting.Models;

namespace PlantSafety.Application.Compliance.Permits.Lifting.Queries;

/// <summary>Result of <see cref="GetLiftingQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Lifting items.</param>
public sealed record LiftingQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<LiftingItemDto> Items);
