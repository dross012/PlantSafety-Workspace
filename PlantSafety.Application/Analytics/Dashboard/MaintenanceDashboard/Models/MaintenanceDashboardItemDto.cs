// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-02
// FILE DESCRIPTION: Read-model item DTO for a single Maintenance BI dashboard KPI row, sourced
//                    from the unified analytics FactMaintenanceWorkOrder fact table.
// SchemaVersion: 1

namespace PlantSafety.Application.Analytics.Dashboard.MaintenanceDashboard.Models;

/// <summary>A single Maintenance dashboard KPI read-model item, aggregated per business date.</summary>
public sealed record MaintenanceDashboardItemDto
{
    /// <summary>Business date this KPI row summarizes.</summary>
    public required DateOnly BusinessDate { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Total maintenance work orders due/scheduled on this date.</summary>
    public required int TotalWorkOrders { get; init; }

    /// <summary>Count of preventive maintenance work orders.</summary>
    public required int PreventiveCount { get; init; }

    /// <summary>Count of predictive maintenance work orders.</summary>
    public required int PredictiveCount { get; init; }

    /// <summary>Count of work orders completed on schedule.</summary>
    public required int OnScheduleCount { get; init; }

    /// <summary>Count of overdue work orders.</summary>
    public required int OverdueCount { get; init; }

    /// <summary>PM/PdM schedule compliance rate for this date, 0.0-1.0.</summary>
    public required double ScheduleComplianceRate { get; init; }

    /// <summary>Total downtime minutes attributed to work orders on this date.</summary>
    public required int TotalDowntimeMinutes { get; init; }

    /// <summary>Average predicted failure probability across predictive-maintenance work orders (0.0-1.0), if any.</summary>
    public double? AveragePredictedFailureProbability { get; init; }
}
