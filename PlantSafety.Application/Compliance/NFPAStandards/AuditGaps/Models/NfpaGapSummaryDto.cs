// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Aggregate counts backing an "NFPA gaps for this site/module" summary tile.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.NFPAStandards.AuditGaps.Models;

/// <summary>Aggregate NFPA gap counts for one module at one site.</summary>
public sealed record NfpaGapSummaryDto
{
    /// <summary>The module these counts apply to.</summary>
    public required string Module { get; init; }

    /// <summary>Total hazards mapped to this module (regardless of coverage).</summary>
    public required int TotalHazards { get; init; }

    /// <summary>Hazards covered by at least one AuditFinding at this site.</summary>
    public required int CoveredCount { get; init; }

    /// <summary>Hazards with no covering AuditFinding at this site.</summary>
    public required int GapCount { get; init; }

    /// <summary>Of the gaps, how many are High severity.</summary>
    public required int HighGapCount { get; init; }

    /// <summary>Of the gaps, how many are Critical severity.</summary>
    public required int CriticalGapCount { get; init; }
}
