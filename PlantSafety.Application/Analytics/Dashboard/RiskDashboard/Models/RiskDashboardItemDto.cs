// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-02
// FILE DESCRIPTION: Read-model item DTO for a single Risk BI dashboard KPI row, sourced from the
//                    unified analytics FactRiskAssessment fact table.
// SchemaVersion: 1

namespace PlantSafety.Application.Analytics.Dashboard.RiskDashboard.Models;

/// <summary>A single Risk dashboard KPI read-model item, aggregated per business date.</summary>
public sealed record RiskDashboardItemDto
{
    /// <summary>Business date this KPI row summarizes.</summary>
    public required DateOnly BusinessDate { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Total risk assessments scored/reviewed on this date.</summary>
    public required int TotalAssessments { get; init; }

    /// <summary>Count of Critical risk level assessments.</summary>
    public required int CriticalCount { get; init; }

    /// <summary>Count of High risk level assessments.</summary>
    public required int HighCount { get; init; }

    /// <summary>Count of Medium risk level assessments.</summary>
    public required int MediumCount { get; init; }

    /// <summary>Count of Low risk level assessments.</summary>
    public required int LowCount { get; init; }

    /// <summary>Average composite Severity x Likelihood score across assessments on this date.</summary>
    public required double AverageCompositeScore { get; init; }

    /// <summary>Count of residual-score assessments recorded (post-control validation) on this date.</summary>
    public required int ResidualAssessmentCount { get; init; }
}
