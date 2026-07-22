// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetPMSchedulingQuery.
// SchemaVersion: 1

using PlantSafety.Application.Scheduling.PMScheduling.Models;

namespace PlantSafety.Application.Scheduling.PMScheduling.Queries;

/// <summary>Result of <see cref="GetPMSchedulingQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching PMScheduling items.</param>
public sealed record PMSchedulingQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<PMSchedulingItemDto> Items);