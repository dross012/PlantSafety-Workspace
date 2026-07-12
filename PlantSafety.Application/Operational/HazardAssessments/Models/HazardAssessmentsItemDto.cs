// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single HazardAssessments record.
// SchemaVersion: 1

namespace PlantSafety.Application.Operational.HazardAssessments.Models;

/// <summary>A single HazardAssessments read-model record.</summary>
public sealed record HazardAssessmentsItemDto
{
    /// <summary>HazardAssessments record identifier.</summary>
    public required Guid HazardAssessmentId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Description of the identified hazard.</summary>
    public required string HazardDescription { get; init; }

    /// <summary>Risk level classification (e.g., Low/Medium/High).</summary>
    public required string RiskLevel { get; init; }

    /// <summary>Current status of this record.</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when this record was last actioned.</summary>
    public required DateTime AssessedAtUtc { get; init; }

    /// <summary>Identity of the user who last actioned this record.</summary>
    public required Guid AssessedBy { get; init; }
}
