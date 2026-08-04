// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-model item DTO for a single CommunityImpactAssessment record.
// SchemaVersion: 1

namespace PlantSafety.Application.CommunitySafety.CommunityImpactAssessment.Models;

/// <summary>A single CommunityImpactAssessment record for an on-site SafetyEvents incident.</summary>
public sealed record CommunityImpactAssessmentItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid CommunityImpactAssessmentId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Weak reference to the SafetyEvents incident this assessment relates to.</summary>
    public required Guid IncidentId { get; init; }

    /// <summary>Assessed severity of the community impact.</summary>
    public required string Severity { get; init; }

    /// <summary>Geographic/community area affected.</summary>
    public required string AffectedArea { get; init; }

    /// <summary>Estimated number of community members affected, if known.</summary>
    public int? EstimatedPersonsAffected { get; init; }

    /// <summary>Narrative description of the assessed impact.</summary>
    public required string Description { get; init; }

    /// <summary>Whether this assessment triggers EPCRA/community right-to-know reporting obligations.</summary>
    public required bool RequiresRightToKnowReporting { get; init; }

    /// <summary>Overall lifecycle status.</summary>
    public required string AssessmentStatus { get; init; }

    /// <summary>UTC instant the assessment was recorded.</summary>
    public required DateTime AssessedAtUtc { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}
