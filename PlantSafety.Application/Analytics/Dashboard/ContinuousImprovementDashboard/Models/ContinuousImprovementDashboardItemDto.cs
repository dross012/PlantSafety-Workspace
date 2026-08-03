// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-02
// FILE DESCRIPTION: Read-model item DTO for a single ContinuousImprovement BI dashboard KPI row,
//                    sourced from the unified analytics FactContinuousImprovementAction fact table.
// SchemaVersion: 1

namespace PlantSafety.Application.Analytics.Dashboard.ContinuousImprovementDashboard.Models;

/// <summary>A single ContinuousImprovement dashboard KPI read-model item, aggregated per business date.</summary>
public sealed record ContinuousImprovementDashboardItemDto
{
    /// <summary>Business date this KPI row summarizes.</summary>
    public required DateOnly BusinessDate { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Total CAPA actions opened on this date.</summary>
    public required int CapaOpenedCount { get; init; }

    /// <summary>Total CAPA actions closed on this date.</summary>
    public required int CapaClosedCount { get; init; }

    /// <summary>Total RCA findings raised on this date.</summary>
    public required int RcaFindingCount { get; init; }

    /// <summary>Count of actions/findings closed on or before their due date.</summary>
    public required int ClosedOnTimeCount { get; init; }

    /// <summary>On-time closure rate for this date, 0.0-1.0.</summary>
    public required double OnTimeClosureRate { get; init; }

    /// <summary>Average cycle time in days for items closed on this date.</summary>
    public required double AverageCycleTimeDays { get; init; }

    /// <summary>Count of corrective actions whose effectiveness has been verified.</summary>
    public required int EffectivenessVerifiedCount { get; init; }
}
