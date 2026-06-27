// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-side DTO for the Safety Dashboard KPI snapshot.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.Analytics.Models.Dashboard;

/// <summary>
/// Top-level Safety Dashboard DTO aggregating KPIs across modules for a given tenant and site.
/// All counts are scoped to the requested time window.
/// </summary>
public sealed record SafetyDashboardDto
{
    /// <summary>Tenant this snapshot belongs to.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site this snapshot belongs to. Null means tenant-wide roll-up.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>UTC timestamp when this snapshot was generated.</summary>
    public required DateTimeOffset SnapshotGeneratedAtUtc { get; init; }

    /// <summary>Start of the reporting window (inclusive).</summary>
    public required DateTimeOffset WindowFromUtc { get; init; }

    /// <summary>End of the reporting window (inclusive).</summary>
    public required DateTimeOffset WindowToUtc { get; init; }

    // --- Incident KPIs ---

    /// <summary>Total incidents in the window.</summary>
    public required int TotalIncidents { get; init; }

    /// <summary>Open (not closed) incidents.</summary>
    public required int OpenIncidents { get; init; }

    /// <summary>Critical severity incidents.</summary>
    public required int CriticalIncidents { get; init; }

    /// <summary>Lost-Time Injury count.</summary>
    public required int LostTimeInjuries { get; init; }

    // --- Near Miss KPIs ---

    /// <summary>Total near misses in the window.</summary>
    public required int TotalNearMisses { get; init; }

    // --- Observation KPIs ---

    /// <summary>Total observations in the window.</summary>
    public required int TotalObservations { get; init; }

    /// <summary>Positive observations in the window.</summary>
    public required int PositiveObservations { get; init; }

    /// <summary>At-risk or unsafe observations in the window.</summary>
    public required int NegativeObservations { get; init; }

    // --- Compliance KPIs ---

    /// <summary>Open permits in the window.</summary>
    public required int OpenPermits { get; init; }

    /// <summary>Overdue compliance obligations.</summary>
    public required int OverdueObligations { get; init; }

    // --- Culture Index ---

    /// <summary>Safety culture score (0.0–10.0), or null if not calculated.</summary>
    public decimal? CultureScore { get; init; }
}
