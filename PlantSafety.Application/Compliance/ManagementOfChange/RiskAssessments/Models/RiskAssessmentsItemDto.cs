// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single Management of Change risk assessment record.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.ManagementOfChange.RiskAssessments.Models;

/// <summary>A single risk assessment performed against a change request.</summary>
public sealed record RiskAssessmentsItemDto
{
    /// <summary>Risk assessment identifier.</summary>
    public required Guid RiskAssessmentId { get; init; }

    /// <summary>Identifier of the change request this assessment applies to.</summary>
    public required Guid ChangeRequestId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Assessed risk level (e.g., Low/Medium/High/Critical).</summary>
    public required string RiskLevel { get; init; }

    /// <summary>Current assessment status (e.g., Draft/InReview/Finalized).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the risk assessment was performed.</summary>
    public required DateTime AssessedAtUtc { get; init; }

    /// <summary>Identity of the user who performed the risk assessment.</summary>
    public required Guid AssessedBy { get; init; }
}
