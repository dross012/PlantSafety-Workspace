// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Total FMCSA requirement counts for one module, independent of any
//   tenant/site coverage -- "total FMCSA requirements per module" tile. Grouped by
//   Compliance.FMCSARequirement's own denormalized AppliesToModule column, not the mapping table
//   (see usp_FmcsaDashboard_GetModuleTotals.sql's header).
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.FMCSAStandards.Dashboards.Models;

/// <summary>Total/High/Critical requirement counts for one PlantSafety module.</summary>
public sealed record FmcsaModuleTotalDto
{
    /// <summary>The module these counts apply to.</summary>
    public required string Module { get; init; }

    /// <summary>Total requirements mapped to this module.</summary>
    public required int TotalRequirements { get; init; }

    /// <summary>Of those, how many are High severity.</summary>
    public required int HighCount { get; init; }

    /// <summary>Of those, how many are Critical severity.</summary>
    public required int CriticalCount { get; init; }
}
