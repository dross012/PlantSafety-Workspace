// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetSiteOrientationQuery.
// SchemaVersion: 1

using PlantSafety.Application.ContractorSafety.SiteOrientation.Models;

namespace PlantSafety.Application.ContractorSafety.SiteOrientation.Queries;

/// <summary>Result of <see cref="GetSiteOrientationQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching SiteOrientation items.</param>
public sealed record SiteOrientationQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<SiteOrientationItemDto> Items);