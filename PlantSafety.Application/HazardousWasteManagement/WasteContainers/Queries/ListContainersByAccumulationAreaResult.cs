// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Result payload for ListContainersByAccumulationAreaQuery.
// SchemaVersion: 1

using PlantSafety.Application.HazardousWasteManagement.WasteContainers.Models;

namespace PlantSafety.Application.HazardousWasteManagement.WasteContainers.Queries;

/// <summary>Result of <see cref="ListContainersByAccumulationAreaQuery"/>.</summary>
/// <param name="Items">Matching WasteContainer items.</param>
public sealed record ListContainersByAccumulationAreaResult(IReadOnlyList<WasteContainerItemDto> Items);
