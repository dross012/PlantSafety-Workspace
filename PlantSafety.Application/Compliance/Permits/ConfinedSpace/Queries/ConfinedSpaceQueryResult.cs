// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetConfinedSpaceQuery.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Permits.ConfinedSpace.Models;

namespace PlantSafety.Application.Compliance.Permits.ConfinedSpace.Queries;

/// <summary>Result of <see cref="GetConfinedSpaceQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching ConfinedSpace items.</param>
public sealed record ConfinedSpaceQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<ConfinedSpaceItemDto> Items);
