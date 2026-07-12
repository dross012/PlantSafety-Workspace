// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetHazardousMaterialsQuery.
// SchemaVersion: 1

using PlantSafety.Application.Operational.HazardousMaterials.Models;

namespace PlantSafety.Application.Operational.HazardousMaterials.Queries;

/// <summary>Result of <see cref="GetHazardousMaterialsQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching HazardousMaterials items.</param>
public sealed record HazardousMaterialsQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<HazardousMaterialsItemDto> Items);
