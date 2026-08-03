// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-02
// FILE DESCRIPTION: Repository abstraction for the Maintenance BI dashboard read-model.
// SchemaVersion: 1

using PlantSafety.Application.Analytics.Dashboard.MaintenanceDashboard.Models;

namespace PlantSafety.Application.Analytics.Dashboard.MaintenanceDashboard.Repositories;

/// <summary>Read-only repository for Maintenance dashboard KPI data. Implemented in the Infrastructure layer.</summary>
public interface IMaintenanceDashboardRepository
{
    /// <summary>Returns Maintenance dashboard KPI items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<MaintenanceDashboardItemDto>> GetMaintenanceDashboardAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
