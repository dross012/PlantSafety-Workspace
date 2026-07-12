// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetRCAQuery.
// SchemaVersion: 1

using PlantSafety.Application.ContinuousImprovement.RCA.Models;

namespace PlantSafety.Application.ContinuousImprovement.RCA.Queries;

/// <summary>Result of <see cref="GetRCAQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching RCA items.</param>
public sealed record RCAQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<RCAItemDto> Items);
