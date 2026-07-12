// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Result payload for GetDrillsAndExercisesQuery.
// SchemaVersion: 1

using PlantSafety.Application.SafetyEvents.DrillsAndExercises.Models;

namespace PlantSafety.Application.SafetyEvents.DrillsAndExercises.Queries;

/// <summary>Result of <see cref="GetDrillsAndExercisesQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching DrillsAndExercises items.</param>
public sealed record DrillsAndExercisesQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<DrillsAndExercisesItemDto> Items);
