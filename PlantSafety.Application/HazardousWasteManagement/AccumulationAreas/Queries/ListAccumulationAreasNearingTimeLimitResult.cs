// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Result payload for ListAccumulationAreasNearingTimeLimitQuery.
// SchemaVersion: 1

using PlantSafety.Application.HazardousWasteManagement.AccumulationAreas.Models;

namespace PlantSafety.Application.HazardousWasteManagement.AccumulationAreas.Queries;

/// <summary>Result of <see cref="ListAccumulationAreasNearingTimeLimitQuery"/>.</summary>
/// <param name="Items">Matching accumulation area items.</param>
public sealed record ListAccumulationAreasNearingTimeLimitResult(IReadOnlyList<AccumulationAreaItemDto> Items);
