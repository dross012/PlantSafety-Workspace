// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetCAPAQuery.
// SchemaVersion: 1

using PlantSafety.Application.ContinuousImprovement.CAPA.Models;

namespace PlantSafety.Application.ContinuousImprovement.CAPA.Queries;

/// <summary>Result of <see cref="GetCAPAQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching CAPA items.</param>
public sealed record CAPAQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<CAPAItemDto> Items);
