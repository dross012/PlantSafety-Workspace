// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetPPECategoryQuery.
// SchemaVersion: 1

using PlantSafety.Application.PPE.PPECategory.Models;

namespace PlantSafety.Application.PPE.PPECategory.Queries;

/// <summary>Result of <see cref="GetPPECategoryQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching PPECategory items.</param>
public sealed record PPECategoryQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<PPECategoryItemDto> Items);