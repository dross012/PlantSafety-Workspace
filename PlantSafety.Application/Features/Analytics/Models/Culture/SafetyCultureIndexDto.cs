// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-side DTO for the Safety Culture Index calculation result.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.Analytics.Models.Culture;

/// <summary>
/// Computed Safety Culture Index for a tenant/site over a reporting window.
/// Derived from observations, near miss reporting rates, training compliance, and engagement metrics.
/// </summary>
public sealed record SafetyCultureIndexDto
{
    /// <summary>Tenant this index belongs to.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site this index belongs to. Null means tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Overall safety culture score (0.0–10.0).</summary>
    public required decimal OverallScore { get; init; }

    /// <summary>Observation participation score sub-component.</summary>
    public required decimal ObservationParticipationScore { get; init; }

    /// <summary>Near miss reporting rate sub-component.</summary>
    public required decimal NearMissReportingScore { get; init; }

    /// <summary>Training compliance sub-component.</summary>
    public required decimal TrainingComplianceScore { get; init; }

    /// <summary>Corrective action closure rate sub-component.</summary>
    public required decimal CorrectiveActionClosureScore { get; init; }

    /// <summary>UTC timestamp of the reporting window start.</summary>
    public required DateTimeOffset WindowFromUtc { get; init; }

    /// <summary>UTC timestamp of the reporting window end.</summary>
    public required DateTimeOffset WindowToUtc { get; init; }

    /// <summary>UTC timestamp when this index was calculated.</summary>
    public required DateTimeOffset CalculatedAtUtc { get; init; }
}
