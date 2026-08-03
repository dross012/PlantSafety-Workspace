// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-02
// FILE DESCRIPTION: Read-model item DTO for a single Audit BI dashboard KPI row, sourced from
//                    the unified analytics FactAuditFinding fact table.
// SchemaVersion: 1

namespace PlantSafety.Application.Analytics.Dashboard.AuditDashboard.Models;

/// <summary>A single Audit dashboard KPI read-model item, aggregated per business date.</summary>
public sealed record AuditDashboardItemDto
{
    /// <summary>Business date this KPI row summarizes.</summary>
    public required DateOnly BusinessDate { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Total audit findings raised on this date.</summary>
    public required int TotalFindingsRaised { get; init; }

    /// <summary>Count of Critical-severity findings raised on this date.</summary>
    public required int CriticalFindingCount { get; init; }

    /// <summary>Count of Major-severity findings raised on this date.</summary>
    public required int MajorFindingCount { get; init; }

    /// <summary>Count of Minor-severity findings raised on this date.</summary>
    public required int MinorFindingCount { get; init; }

    /// <summary>Count of findings closed/remediated on this date.</summary>
    public required int FindingsClosedCount { get; init; }

    /// <summary>Count of findings remediated on or before their due date.</summary>
    public required int RemediatedOnTimeCount { get; init; }

    /// <summary>On-time remediation rate for this date, 0.0-1.0.</summary>
    public required double OnTimeRemediationRate { get; init; }

    /// <summary>Average remediation cycle time in days for findings closed on this date.</summary>
    public required double AverageRemediationDays { get; init; }
}
