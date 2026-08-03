// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-02
// FILE DESCRIPTION: Client-side item DTOs for the Phase 5 §4.3 BI Dashboards
//   sub-features (Scheduling/Production/Maintenance/Risk/ContinuousImprovement/
//   Audit). Mirror the PlantSafety.Application.Analytics.Dashboard.*Dashboard
//   item DTOs field-for-field so BiDashboardsApiClient can deserialize the
//   real backend responses from api/v1/analytics/dashboard/{scheduling,
//   production,maintenance,risk,continuousimprovement,audit} directly, once
//   those sub-feature controllers are added under
//   PlantSafety.Api/Analytics/Dashboard/&lt;SubFeature&gt;/Controllers (mirroring
//   the existing Executive/KPISnapshots/ReadModel/TrendAnalysis controllers).
//   Display-only — no domain logic.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;

namespace PlantSafety.UI.Modules.BiDashboards.Models;

/// <summary>A single Scheduling BI dashboard KPI row, aggregated per business date (mirrors <c>SchedulingDashboardItemDto</c>).</summary>
public sealed record SchedulingDashboardItemDto
{
    /// <summary>Business date this KPI row summarizes.</summary>
    public DateOnly BusinessDate { get; init; }

    /// <summary>Tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Total scheduled events on this date across all Scheduling sub-contexts.</summary>
    public int TotalScheduledEvents { get; init; }

    /// <summary>Count of events completed on time.</summary>
    public int OnTimeCompletedCount { get; init; }

    /// <summary>Count of events completed late.</summary>
    public int LateCompletedCount { get; init; }

    /// <summary>On-time completion rate for this date, 0.0-1.0.</summary>
    public double OnTimeRate { get; init; }

    /// <summary>Average completion variance in minutes (positive = late).</summary>
    public double AverageVarianceMinutes { get; init; }

    /// <summary>Count of P1 (highest priority) scheduling events outstanding.</summary>
    public int P1OutstandingCount { get; init; }
}

/// <summary>A single Production/Operational BI dashboard KPI row, aggregated per business date (mirrors <c>ProductionDashboardItemDto</c>).</summary>
public sealed record ProductionDashboardItemDto
{
    /// <summary>Business date this KPI row summarizes.</summary>
    public DateOnly BusinessDate { get; init; }

    /// <summary>Tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Total operational events recorded on this date (inspections, checklists, JSAs, PPE checks).</summary>
    public int TotalOperationalEvents { get; init; }

    /// <summary>Count of events that were fully compliant.</summary>
    public int CompliantEventCount { get; init; }

    /// <summary>Count of events with one or more discrepancies/non-conformances.</summary>
    public int NonCompliantEventCount { get; init; }

    /// <summary>Compliance rate for this date, 0.0-1.0.</summary>
    public double ComplianceRate { get; init; }

    /// <summary>Total discrepancies/non-conformances recorded on this date.</summary>
    public int TotalDiscrepancies { get; init; }
}

/// <summary>A single Maintenance BI dashboard KPI row, aggregated per business date (mirrors <c>MaintenanceDashboardItemDto</c>).</summary>
public sealed record MaintenanceDashboardItemDto
{
    /// <summary>Business date this KPI row summarizes.</summary>
    public DateOnly BusinessDate { get; init; }

    /// <summary>Tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Total maintenance work orders due/scheduled on this date.</summary>
    public int TotalWorkOrders { get; init; }

    /// <summary>Count of preventive maintenance work orders.</summary>
    public int PreventiveCount { get; init; }

    /// <summary>Count of predictive maintenance work orders.</summary>
    public int PredictiveCount { get; init; }

    /// <summary>Count of work orders completed on schedule.</summary>
    public int OnScheduleCount { get; init; }

    /// <summary>Count of overdue work orders.</summary>
    public int OverdueCount { get; init; }

    /// <summary>PM/PdM schedule compliance rate for this date, 0.0-1.0.</summary>
    public double ScheduleComplianceRate { get; init; }

    /// <summary>Total downtime minutes attributed to work orders on this date.</summary>
    public int TotalDowntimeMinutes { get; init; }

    /// <summary>Average predicted failure probability across predictive-maintenance work orders (0.0-1.0), if any.</summary>
    public double? AveragePredictedFailureProbability { get; init; }
}

/// <summary>A single Risk BI dashboard KPI row, aggregated per business date (mirrors <c>RiskDashboardItemDto</c>).</summary>
public sealed record RiskDashboardItemDto
{
    /// <summary>Business date this KPI row summarizes.</summary>
    public DateOnly BusinessDate { get; init; }

    /// <summary>Tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Total risk assessments scored/reviewed on this date.</summary>
    public int TotalAssessments { get; init; }

    /// <summary>Count of Critical risk level assessments.</summary>
    public int CriticalCount { get; init; }

    /// <summary>Count of High risk level assessments.</summary>
    public int HighCount { get; init; }

    /// <summary>Count of Medium risk level assessments.</summary>
    public int MediumCount { get; init; }

    /// <summary>Count of Low risk level assessments.</summary>
    public int LowCount { get; init; }

    /// <summary>Average composite Severity x Likelihood score across assessments on this date.</summary>
    public double AverageCompositeScore { get; init; }

    /// <summary>Count of residual-score assessments recorded (post-control validation) on this date.</summary>
    public int ResidualAssessmentCount { get; init; }
}

/// <summary>A single ContinuousImprovement BI dashboard KPI row, aggregated per business date (mirrors <c>ContinuousImprovementDashboardItemDto</c>).</summary>
public sealed record ContinuousImprovementDashboardItemDto
{
    /// <summary>Business date this KPI row summarizes.</summary>
    public DateOnly BusinessDate { get; init; }

    /// <summary>Tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Total CAPA actions opened on this date.</summary>
    public int CapaOpenedCount { get; init; }

    /// <summary>Total CAPA actions closed on this date.</summary>
    public int CapaClosedCount { get; init; }

    /// <summary>Total RCA findings raised on this date.</summary>
    public int RcaFindingCount { get; init; }

    /// <summary>Count of actions/findings closed on or before their due date.</summary>
    public int ClosedOnTimeCount { get; init; }

    /// <summary>On-time closure rate for this date, 0.0-1.0.</summary>
    public double OnTimeClosureRate { get; init; }

    /// <summary>Average cycle time in days for items closed on this date.</summary>
    public double AverageCycleTimeDays { get; init; }

    /// <summary>Count of corrective actions whose effectiveness has been verified.</summary>
    public int EffectivenessVerifiedCount { get; init; }
}

/// <summary>A single Audit BI dashboard KPI row, aggregated per business date (mirrors <c>AuditDashboardItemDto</c>).</summary>
public sealed record AuditDashboardItemDto
{
    /// <summary>Business date this KPI row summarizes.</summary>
    public DateOnly BusinessDate { get; init; }

    /// <summary>Tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Total audit findings raised on this date.</summary>
    public int TotalFindingsRaised { get; init; }

    /// <summary>Count of Critical-severity findings raised on this date.</summary>
    public int CriticalFindingCount { get; init; }

    /// <summary>Count of Major-severity findings raised on this date.</summary>
    public int MajorFindingCount { get; init; }

    /// <summary>Count of Minor-severity findings raised on this date.</summary>
    public int MinorFindingCount { get; init; }

    /// <summary>Count of findings closed/remediated on this date.</summary>
    public int FindingsClosedCount { get; init; }

    /// <summary>Count of findings remediated on or before their due date.</summary>
    public int RemediatedOnTimeCount { get; init; }

    /// <summary>On-time remediation rate for this date, 0.0-1.0.</summary>
    public double OnTimeRemediationRate { get; init; }

    /// <summary>Average remediation cycle time in days for findings closed on this date.</summary>
    public double AverageRemediationDays { get; init; }
}
