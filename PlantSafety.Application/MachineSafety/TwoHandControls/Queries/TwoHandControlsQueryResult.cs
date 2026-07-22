// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetTwoHandControlsQuery.
// SchemaVersion: 1

using PlantSafety.Application.MachineSafety.TwoHandControls.Models;

namespace PlantSafety.Application.MachineSafety.TwoHandControls.Queries;

/// <summary>Result of <see cref="GetTwoHandControlsQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching TwoHandControls items.</param>
public sealed record TwoHandControlsQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<TwoHandControlsItemDto> Items);