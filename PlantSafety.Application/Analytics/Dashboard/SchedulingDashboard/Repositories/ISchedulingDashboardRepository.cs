// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-02
// FILE DESCRIPTION: Repository abstraction for the Scheduling BI dashboard read-model.
// SchemaVersion: 1

using PlantSafety.Application.Analytics.Dashboard.SchedulingDashboard.Models;

namespace PlantSafety.Application.Analytics.Dashboard.SchedulingDashboard.Repositories;

/// <summary>Read-only repository for Scheduling dashboard KPI data. Implemented in the Infrastructure layer.</summary>
public interface ISchedulingDashboardRepository
{
    /// <summary>Returns Scheduling dashboard KPI items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<SchedulingDashboardItemDto>> GetSchedulingDashboardAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
