// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-02
// FILE DESCRIPTION: Read-model item DTO for a single Scheduling BI dashboard KPI row, sourced
//                    from the unified analytics FactSchedulingEvent fact table.
// SchemaVersion: 1

namespace PlantSafety.Application.Analytics.Dashboard.SchedulingDashboard.Models;

/// <summary>A single Scheduling dashboard KPI read-model item, aggregated per business date.</summary>
public sealed record SchedulingDashboardItemDto
{
    /// <summary>Business date this KPI row summarizes.</summary>
    public required DateOnly BusinessDate { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Total scheduled events on this date across all Scheduling sub-contexts.</summary>
    public required int TotalScheduledEvents { get; init; }

    /// <summary>Count of events completed on time.</summary>
    public required int OnTimeCompletedCount { get; init; }

    /// <summary>Count of events completed late.</summary>
    public required int LateCompletedCount { get; init; }

    /// <summary>On-time completion rate for this date, 0.0-1.0.</summary>
    public required double OnTimeRate { get; init; }

    /// <summary>Average completion variance in minutes (positive = late).</summary>
    public required double AverageVarianceMinutes { get; init; }

    /// <summary>Count of P1 (highest priority) scheduling events outstanding.</summary>
    public required int P1OutstandingCount { get; init; }
}
