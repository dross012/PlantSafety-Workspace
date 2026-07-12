// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetWorkatHeightQuery.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Permits.WorkatHeight.Models;

namespace PlantSafety.Application.Compliance.Permits.WorkatHeight.Queries;

/// <summary>Result of <see cref="GetWorkatHeightQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching WorkatHeight items.</param>
public sealed record WorkatHeightQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<WorkatHeightItemDto> Items);
