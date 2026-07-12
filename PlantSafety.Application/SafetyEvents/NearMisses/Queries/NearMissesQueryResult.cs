// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetNearMissesQuery.
// SchemaVersion: 1

using PlantSafety.Application.SafetyEvents.NearMisses.Models;

namespace PlantSafety.Application.SafetyEvents.NearMisses.Queries;

/// <summary>Result of <see cref="GetNearMissesQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching NearMisses items.</param>
public sealed record NearMissesQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<NearMissesItemDto> Items);
