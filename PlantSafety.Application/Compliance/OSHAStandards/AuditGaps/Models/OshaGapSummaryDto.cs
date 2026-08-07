// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Aggregate counts backing an "OSHA gaps for this site/module" summary tile.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.OSHAStandards.AuditGaps.Models;

/// <summary>Aggregate OSHA gap counts for one module at one site.</summary>
public sealed record OshaGapSummaryDto
{
    /// <summary>The module these counts apply to.</summary>
    public required string Module { get; init; }

    /// <summary>Total requirements mapped to this module (regardless of coverage).</summary>
    public required int TotalRequirements { get; init; }

    /// <summary>Requirements covered by at least one AuditFinding at this site.</summary>
    public required int CoveredCount { get; init; }

    /// <summary>Requirements with no covering AuditFinding at this site.</summary>
    public required int GapCount { get; init; }

    /// <summary>Of the gaps, how many are High severity.</summary>
    public required int HighGapCount { get; init; }

    /// <summary>Of the gaps, how many are Critical severity.</summary>
    public required int CriticalGapCount { get; init; }
}
