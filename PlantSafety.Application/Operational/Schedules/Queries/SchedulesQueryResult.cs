// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetSchedulesQuery.
// SchemaVersion: 1

using PlantSafety.Application.Operational.Schedules.Models;

namespace PlantSafety.Application.Operational.Schedules.Queries;

/// <summary>Result of <see cref="GetSchedulesQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching Schedules items.</param>
public sealed record SchedulesQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<SchedulesItemDto> Items);
