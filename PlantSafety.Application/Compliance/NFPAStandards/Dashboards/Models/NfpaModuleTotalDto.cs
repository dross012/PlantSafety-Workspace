// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Total NFPA hazard-mapping counts for one module, independent of any
//   tenant/site coverage -- "total NFPA hazards per module" tile.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.NFPAStandards.Dashboards.Models;

/// <summary>Total/High/Critical hazard-mapping counts for one PlantSafety module.</summary>
public sealed record NfpaModuleTotalDto
{
    /// <summary>The module these counts apply to.</summary>
    public required string Module { get; init; }

    /// <summary>Total hazards mapped to this module.</summary>
    public required int TotalHazards { get; init; }

    /// <summary>Of those, how many are High severity.</summary>
    public required int HighCount { get; init; }

    /// <summary>Of those, how many are Critical severity.</summary>
    public required int CriticalCount { get; init; }
}
