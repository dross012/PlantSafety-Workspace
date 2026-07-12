// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single fatigue risk assessment.
// SchemaVersion: 1

namespace PlantSafety.Application.Analytics.Predictive.FatigueRiskModeling.Models;

/// <summary>A single fatigue risk assessment produced for an employee.</summary>
public sealed record FatigueRiskModelingItemDto
{
    /// <summary>Risk assessment identifier.</summary>
    public required Guid RiskAssessmentId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Identifier of the assessed employee.</summary>
    public required Guid EmployeeId { get; init; }

    /// <summary>Computed fatigue risk score.</summary>
    public required decimal FatigueRiskScore { get; init; }

    /// <summary>Risk classification (e.g., Low, Medium, High, Critical).</summary>
    public required string RiskLevel { get; init; }

    /// <summary>Current status of the assessment (e.g., Active, Reviewed, Closed).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the assessment was performed.</summary>
    public required DateTime AssessedAtUtc { get; init; }
}
