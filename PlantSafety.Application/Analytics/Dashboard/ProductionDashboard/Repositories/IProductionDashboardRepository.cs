// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-02
// FILE DESCRIPTION: Repository abstraction for the Production/Operational BI dashboard read-model.
// SchemaVersion: 1

using PlantSafety.Application.Analytics.Dashboard.ProductionDashboard.Models;

namespace PlantSafety.Application.Analytics.Dashboard.ProductionDashboard.Repositories;

/// <summary>Read-only repository for Production/Operational dashboard KPI data. Implemented in the Infrastructure layer.</summary>
public interface IProductionDashboardRepository
{
    /// <summary>Returns Production/Operational dashboard KPI items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<ProductionDashboardItemDto>> GetProductionDashboardAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
