// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetPPEZoneMappingQuery.
// SchemaVersion: 1

using PlantSafety.Application.PPE.PPEZoneMapping.Models;

namespace PlantSafety.Application.PPE.PPEZoneMapping.Queries;

/// <summary>Result of <see cref="GetPPEZoneMappingQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching PPEZoneMapping items.</param>
public sealed record PPEZoneMappingQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<PPEZoneMappingItemDto> Items);