// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Result payload for ListActiveAnomaliesQuery.
// SchemaVersion: 1

using PlantSafety.Application.PhysicalAccessControl.AccessAnomalies.Models;

namespace PlantSafety.Application.PhysicalAccessControl.AccessAnomalies.Queries;

/// <summary>Result of <see cref="ListActiveAnomaliesQuery"/>.</summary>
/// <param name="Items">Matching active AccessAnomaly items.</param>
public sealed record ListActiveAnomaliesResult(IReadOnlyList<AccessAnomalyDto> Items);
