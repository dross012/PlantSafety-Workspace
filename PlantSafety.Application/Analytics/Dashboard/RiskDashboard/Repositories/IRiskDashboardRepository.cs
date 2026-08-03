// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-02
// FILE DESCRIPTION: Repository abstraction for the Risk BI dashboard read-model.
// SchemaVersion: 1

using PlantSafety.Application.Analytics.Dashboard.RiskDashboard.Models;

namespace PlantSafety.Application.Analytics.Dashboard.RiskDashboard.Repositories;

/// <summary>Read-only repository for Risk dashboard KPI data. Implemented in the Infrastructure layer.</summary>
public interface IRiskDashboardRepository
{
    /// <summary>Returns Risk dashboard KPI items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<RiskDashboardItemDto>> GetRiskDashboardAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
