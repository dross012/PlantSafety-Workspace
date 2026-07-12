// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-side repository interface for Safety Dashboard analytics.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.Analytics.Handlers.Dashboard;

/// <summary>Read-only repository for Safety Dashboard analytics.</summary>
public interface IDashboardRepository
{
    /// <summary>Returns the aggregated dashboard metrics for the given tenant and optional site.</summary>
    Task<DashboardReadModel?> GetDashboardAsync(
        Guid tenantId,
        Guid? siteId,
        CancellationToken cancellationToken);

    /// <summary>Returns the safety report metrics for the given tenant, site, and date range.</summary>
    Task<SafetyReportReadModel?> GetReportAsync(
        Guid tenantId,
        Guid? siteId,
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        CancellationToken cancellationToken);
}

/// <summary>Infrastructure read model for Safety Dashboard stored-procedure result sets.</summary>
public sealed record DashboardReadModel
{
    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }
    /// <summary>Site identifier filter applied.</summary>
    public required Guid? SiteId { get; init; }
    /// <summary>Total incidents.</summary>
    public required int TotalIncidents { get; init; }
    /// <summary>Open incidents.</summary>
    public required int OpenIncidents { get; init; }
    /// <summary>Critical incidents.</summary>
    public required int CriticalIncidents { get; init; }
    /// <summary>Lost time injuries.</summary>
    public required int LostTimeInjuries { get; init; }
    /// <summary>Total near misses.</summary>
    public required int TotalNearMisses { get; init; }
    /// <summary>Total observations.</summary>
    public required int TotalObservations { get; init; }
    /// <summary>Positive observations.</summary>
    public required int PositiveObservations { get; init; }
    /// <summary>Negative observations.</summary>
    public required int NegativeObservations { get; init; }
    /// <summary>Open permits.</summary>
    public required int OpenPermits { get; init; }
    /// <summary>Overdue obligations.</summary>
    public required int OverdueObligations { get; init; }
    /// <summary>Safety culture index score (0-10).</summary>
    public required decimal? CultureScore { get; init; }
    /// <summary>Timestamp when this snapshot was calculated.</summary>
    public required DateTimeOffset CalculatedAtUtc { get; init; }
}

/// <summary>Infrastructure read model for Safety Report stored-procedure result sets.</summary>
public sealed record SafetyReportReadModel
{
    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }
    /// <summary>Site identifier filter applied.</summary>
    public required Guid? SiteId { get; init; }
    /// <summary>Report period start.</summary>
    public required DateTimeOffset FromUtc { get; init; }
    /// <summary>Report period end.</summary>
    public required DateTimeOffset ToUtc { get; init; }
    /// <summary>UTC timestamp when this report was generated.</summary>
    public required DateTimeOffset GeneratedAtUtc { get; init; }
    /// <summary>Total Recordable Incident Rate.</summary>
    public required decimal Trir { get; init; }
    /// <summary>Days Away, Restricted, or Transfer rate.</summary>
    public required decimal Dart { get; init; }
    /// <summary>Total incidents in the period.</summary>
    public required int TotalIncidents { get; init; }
    /// <summary>Total near misses in the period.</summary>
    public required int TotalNearMisses { get; init; }
    /// <summary>Total observations in the period.</summary>
    public required int TotalObservations { get; init; }
    /// <summary>Open corrective actions.</summary>
    public required int OpenCAPAs { get; init; }
    /// <summary>Training compliance percentage.</summary>
    public required decimal TrainingComplianceRate { get; init; }
    /// <summary>Safety culture score.</summary>
    public required decimal? CultureScore { get; init; }
}
