// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model item DTO for a single IncidentTrackingPerformanceEvaluation record.
// SchemaVersion: 1

namespace PlantSafety.Application.ContractorSafety.IncidentTrackingPerformanceEvaluation.Models;

/// <summary>A single IncidentTrackingPerformanceEvaluation record (Incident tracking and performance evaluation includes contractors in the facility incident process: investigation, root-cause analysis, CAPA documentation, and eligibility scoring for future work.).</summary>
public sealed record IncidentTrackingPerformanceEvaluationItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid IncidentTrackingPerformanceEvaluationId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Evaluation Title.</summary>
    public required string EvaluationTitle { get; init; }

    /// <summary>Classification: Incident, NearMiss, UnsafeCondition.</summary>
    public required string IncidentCategory { get; init; }

    /// <summary>Contractor Name.</summary>
    public required string ContractorName { get; init; }

    /// <summary>Overall status classification.</summary>
    public required string EligibilityStatus { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}