// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-model item DTO for a single DischargeMonitoringReport record.
// SchemaVersion: 1

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Models;

/// <summary>A single DischargeMonitoringReport read-model record.</summary>
public sealed record DischargeMonitoringReportItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid DischargeMonitoringReportId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Identity of the owning DischargePermit.</summary>
    public required Guid DischargePermitId { get; init; }

    /// <summary>UTC start of the reporting period.</summary>
    public required DateTime ReportingPeriodStartUtc { get; init; }

    /// <summary>UTC end of the reporting period.</summary>
    public required DateTime ReportingPeriodEndUtc { get; init; }

    /// <summary>UTC due date.</summary>
    public required DateTime DueAtUtc { get; init; }

    /// <summary>UTC submission timestamp, if submitted.</summary>
    public DateTime? SubmittedAtUtc { get; init; }

    /// <summary>Count of samples within limit for the period.</summary>
    public required int SamplesWithinLimitCount { get; init; }

    /// <summary>Total samples for the period.</summary>
    public required int TotalSamplesCount { get; init; }

    /// <summary>Lifecycle status.</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}