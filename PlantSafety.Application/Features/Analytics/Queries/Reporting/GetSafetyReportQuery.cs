// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Query to generate a structured Safety Report for a given period.
//   Returns an aggregated metrics report across modules for the specified window.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;

namespace PlantSafety.Application.Features.Analytics.Queries.Reporting;

/// <summary>
/// Generates a Safety Report covering the requested period.
/// The report aggregates data from SafetyEvents, Compliance, Training, and CAPAs.
/// Caller must hold <c>Analytics.Reports.View</c>.
/// </summary>
public sealed record GetSafetyReportQuery : IQuery<SafetyReportDto>
{
    /// <inheritdoc/>
    public required Guid TraceId { get; init; }

    /// <inheritdoc/>
    public required Guid SpanId { get; init; }

    /// <inheritdoc/>
    public required string ApiVersionToken { get; init; }

    /// <inheritdoc/>
    public required Guid RequestingUserId { get; init; }

    /// <inheritdoc/>
    public required string RequestingUserRole { get; init; }

    /// <inheritdoc/>
    public required Guid TenantId { get; init; }

    /// <inheritdoc/>
    public required Guid CorrelationId { get; init; }

    /// <inheritdoc/>
    public CachePreference CachePreference { get; init; } = CachePreference.Preferred;

    /// <inheritdoc/>
    public bool SuppressUnauthorized { get; init; }

    /// <inheritdoc/>
    public bool StrictMode { get; init; }

    /// <summary>Optional site filter. Null = tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Reporting period start (UTC, inclusive).</summary>
    public required DateTimeOffset FromUtc { get; init; }

    /// <summary>Reporting period end (UTC, inclusive).</summary>
    public required DateTimeOffset ToUtc { get; init; }

    /// <summary>Report type (e.g., Monthly, Quarterly, Annual, Custom).</summary>
    public required string ReportType { get; init; }
}

/// <summary>
/// Structured safety report DTO returned by <see cref="GetSafetyReportQuery"/>.
/// </summary>
public sealed record SafetyReportDto
{
    /// <summary>Tenant the report belongs to.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site the report belongs to. Null = tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Report period start.</summary>
    public required DateTimeOffset FromUtc { get; init; }

    /// <summary>Report period end.</summary>
    public required DateTimeOffset ToUtc { get; init; }

    /// <summary>UTC timestamp when the report was generated.</summary>
    public required DateTimeOffset GeneratedAtUtc { get; init; }

    /// <summary>Total recordable incident rate (TRIR).</summary>
    public required decimal TRIR { get; init; }

    /// <summary>Days away/restricted/transfer rate (DART).</summary>
    public required decimal DART { get; init; }

    /// <summary>Total incidents.</summary>
    public required int TotalIncidents { get; init; }

    /// <summary>Total near misses reported.</summary>
    public required int TotalNearMisses { get; init; }

    /// <summary>Total observations submitted.</summary>
    public required int TotalObservations { get; init; }

    /// <summary>Open corrective actions.</summary>
    public required int OpenCAPAs { get; init; }

    /// <summary>Training compliance rate (0–100%).</summary>
    public required decimal TrainingComplianceRate { get; init; }

    /// <summary>Safety culture score (0.0–10.0).</summary>
    public decimal? CultureScore { get; init; }
}
