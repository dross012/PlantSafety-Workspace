// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for GetShiftFatigueQuery.
// SchemaVersion: 1

using PlantSafety.Application.Scheduling.ShiftFatigue.Models;

namespace PlantSafety.Application.Scheduling.ShiftFatigue.Queries;

/// <summary>Result of <see cref="GetShiftFatigueQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching ShiftFatigue items.</param>
public sealed record ShiftFatigueQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<ShiftFatigueItemDto> Items);