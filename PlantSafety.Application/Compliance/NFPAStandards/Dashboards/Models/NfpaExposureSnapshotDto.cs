// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: A single point-in-time exposure snapshot, projected from
//   Compliance.NFPAExposureSnapshot -- the data behind the "exposure trend over time" tile. Used
//   only by the trend read path; live exposure ("right now") is represented by NfpaGapSummaryDto
//   instead, which carries no SnapshotAtUtc -- mirrors OshaExposureSnapshotDto's precedent
//   exactly (OshaGapSummaryDto for live exposure, OshaExposureSnapshotDto only for trend rows).
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.NFPAStandards.Dashboards.Models;

/// <summary>A single NFPA exposure snapshot for one module at one point in time.</summary>
public sealed record NfpaExposureSnapshotDto
{
    /// <summary>UTC instant this snapshot was recorded.</summary>
    public required DateTime SnapshotAtUtc { get; init; }

    /// <summary>The module this snapshot applies to.</summary>
    public required string Module { get; init; }

    /// <summary>Total hazards mapped to this module at snapshot time.</summary>
    public required int TotalHazards { get; init; }

    /// <summary>Hazards covered by at least one AuditFinding at snapshot time.</summary>
    public required int CoveredCount { get; init; }

    /// <summary>Hazards with no covering AuditFinding at snapshot time.</summary>
    public required int GapCount { get; init; }

    /// <summary>Of the gaps, how many were High severity.</summary>
    public required int HighGapCount { get; init; }

    /// <summary>Of the gaps, how many were Critical severity.</summary>
    public required int CriticalGapCount { get; init; }
}
