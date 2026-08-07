// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Total OSHA requirement counts for one module, independent of any
//   tenant/site coverage -- "total OSHA requirements per module" tile.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.OSHAStandards.Dashboards.Models;

/// <summary>Total/High/Critical requirement counts for one PlantSafety module.</summary>
public sealed record OshaModuleTotalDto
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
