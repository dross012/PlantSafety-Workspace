// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: A single point-in-time exposure snapshot, projected from
//   Compliance.OSHAExposureSnapshot -- the data behind the "exposure trend over time" tile.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.OSHAStandards.Dashboards.Models;

/// <summary>A single OSHA exposure snapshot for one module at one point in time.</summary>
public sealed record OshaExposureSnapshotDto
{
    /// <summary>UTC instant this snapshot was recorded.</summary>
    public required DateTime SnapshotAtUtc { get; init; }

    /// <summary>The module this snapshot applies to.</summary>
    public required string Module { get; init; }

    /// <summary>Total requirements mapped to this module at snapshot time.</summary>
    public required int TotalRequirements { get; init; }

    /// <summary>Requirements covered by at least one AuditFinding at snapshot time.</summary>
    public required int CoveredCount { get; init; }

    /// <summary>Requirements with no covering AuditFinding at snapshot time.</summary>
    public required int GapCount { get; init; }

    /// <summary>Of the gaps, how many were High severity.</summary>
    public required int HighGapCount { get; init; }

    /// <summary>Of the gaps, how many were Critical severity.</summary>
    public required int CriticalGapCount { get; init; }
}
