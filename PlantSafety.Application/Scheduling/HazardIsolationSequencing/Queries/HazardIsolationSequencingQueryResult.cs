// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetHazardIsolationSequencingQuery.
// SchemaVersion: 1

using PlantSafety.Application.Scheduling.HazardIsolationSequencing.Models;

namespace PlantSafety.Application.Scheduling.HazardIsolationSequencing.Queries;

/// <summary>Result of <see cref="GetHazardIsolationSequencingQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching HazardIsolationSequencing items.</param>
public sealed record HazardIsolationSequencingQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<HazardIsolationSequencingItemDto> Items);