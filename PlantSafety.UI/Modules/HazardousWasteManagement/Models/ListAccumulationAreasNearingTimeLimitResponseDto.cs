// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-04
// FILE DESCRIPTION: Client-side DTO mirroring PlantSafety.Application.HazardousWasteManagement.
//   AccumulationAreas.Queries.ListAccumulationAreasNearingTimeLimitResult.
// SchemaVersion: 2026.2
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System.Collections.Generic;

namespace PlantSafety.UI.Modules.HazardousWasteManagement.Models;

/// <summary>Response payload for the accumulation-areas-nearing-time-limit read-model query.</summary>
public sealed class ListAccumulationAreasNearingTimeLimitResponseDto
{
    /// <summary>Gets the matching accumulation area records. Never null; empty when no results.</summary>
    public IReadOnlyList<AccumulationAreaDto> Items { get; init; } = [];
}
