// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-side DTO for a Hazard Assessment record.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.Operational.Models.HazardAssessments;

/// <summary>
/// Read-side projection of a HazardAssessment.
/// Formally documents identified workplace hazards, risk ratings, and control measures.
/// </summary>
public sealed record HazardAssessmentDto
{
    /// <summary>Unique identifier (UUIDv7).</summary>
    public required Guid HazardAssessmentId { get; init; }

    /// <summary>Tenant owning this assessment.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site being assessed.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Work area or process being assessed.</summary>
    public required string AssessmentArea { get; init; }

    /// <summary>Hazard description. Max 2000 characters.</summary>
    public required string HazardDescription { get; init; }

    /// <summary>Hazard category (e.g., Physical, Chemical, Biological, Ergonomic, Psychological).</summary>
    public required string HazardCategory { get; init; }

    /// <summary>Likelihood score (1–5).</summary>
    public required int Likelihood { get; init; }

    /// <summary>Consequence score (1–5).</summary>
    public required int Consequence { get; init; }

    /// <summary>Risk score (Likelihood × Consequence).</summary>
    public required int RiskScore { get; init; }

    /// <summary>Risk rating (Low, Medium, High, VeryHigh, Extreme).</summary>
    public required string RiskRating { get; init; }

    /// <summary>Identified control measures. Max 4000 characters.</summary>
    public required string ControlMeasures { get; init; }

    /// <summary>Workflow status (e.g., Draft, Active, UnderReview, Superseded).</summary>
    public required string Status { get; init; }

    /// <summary>Worker who performed the assessment.</summary>
    public required Guid AssessedByActorId { get; init; }

    /// <summary>UTC timestamp when assessment was performed.</summary>
    public required DateTimeOffset AssessedAtUtc { get; init; }

    /// <summary>UTC timestamp when next review is due.</summary>
    public required DateTimeOffset ReviewDueAtUtc { get; init; }

    /// <summary>UTC timestamp of last modification.</summary>
    public required DateTimeOffset LastModifiedAtUtc { get; init; }

    /// <summary>Row version for concurrency.</summary>
    public required string ETag { get; init; }
}
