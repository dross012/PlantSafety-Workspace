// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetWorkCentersQuery.
// SchemaVersion: 1

using PlantSafety.Application.CrossDomain.WorkCenters.Models;

namespace PlantSafety.Application.CrossDomain.WorkCenters.Queries;

/// <summary>Result of <see cref="GetWorkCentersQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching WorkCenters items.</param>
public sealed record WorkCentersQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<WorkCentersItemDto> Items);
