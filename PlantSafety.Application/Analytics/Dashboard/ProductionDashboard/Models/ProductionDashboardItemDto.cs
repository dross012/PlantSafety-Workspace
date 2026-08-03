// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-02
// FILE DESCRIPTION: Read-model item DTO for a single Production/Operational BI dashboard KPI row,
//                    sourced from the unified analytics FactOperationalEvent fact table.
// SchemaVersion: 1

namespace PlantSafety.Application.Analytics.Dashboard.ProductionDashboard.Models;

/// <summary>A single Production/Operational dashboard KPI read-model item, aggregated per business date.</summary>
public sealed record ProductionDashboardItemDto
{
    /// <summary>Business date this KPI row summarizes.</summary>
    public required DateOnly BusinessDate { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Total operational events recorded on this date (inspections, checklists, JSAs, PPE checks).</summary>
    public required int TotalOperationalEvents { get; init; }

    /// <summary>Count of events that were fully compliant.</summary>
    public required int CompliantEventCount { get; init; }

    /// <summary>Count of events with one or more discrepancies/non-conformances.</summary>
    public required int NonCompliantEventCount { get; init; }

    /// <summary>Compliance rate for this date, 0.0-1.0.</summary>
    public required double ComplianceRate { get; init; }

    /// <summary>Total discrepancies/non-conformances recorded on this date.</summary>
    public required int TotalDiscrepancies { get; init; }
}
