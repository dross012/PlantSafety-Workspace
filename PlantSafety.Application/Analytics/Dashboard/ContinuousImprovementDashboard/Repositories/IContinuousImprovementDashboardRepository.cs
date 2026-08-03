// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-02
// FILE DESCRIPTION: Repository abstraction for the ContinuousImprovement BI dashboard read-model.
// SchemaVersion: 1

using PlantSafety.Application.Analytics.Dashboard.ContinuousImprovementDashboard.Models;

namespace PlantSafety.Application.Analytics.Dashboard.ContinuousImprovementDashboard.Repositories;

/// <summary>Read-only repository for ContinuousImprovement dashboard KPI data. Implemented in the Infrastructure layer.</summary>
public interface IContinuousImprovementDashboardRepository
{
    /// <summary>Returns ContinuousImprovement dashboard KPI items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<ContinuousImprovementDashboardItemDto>> GetContinuousImprovementDashboardAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
