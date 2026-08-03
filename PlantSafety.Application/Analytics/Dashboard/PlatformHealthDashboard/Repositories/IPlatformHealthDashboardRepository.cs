// AUTHOR: PlantSafety Platform Team (Claude Code performance-optimization-agent)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Repository abstraction for the PlatformHealth BI dashboard read-model.
// SchemaVersion: 1

using PlantSafety.Application.Analytics.Dashboard.PlatformHealthDashboard.Models;

namespace PlantSafety.Application.Analytics.Dashboard.PlatformHealthDashboard.Repositories;

/// <summary>Read-only repository for PlatformHealth dashboard KPI data. Implemented in the Infrastructure layer.</summary>
public interface IPlatformHealthDashboardRepository
{
    /// <summary>Returns PlatformHealth dashboard KPI items for the tenant within the given UTC date range.</summary>
    Task<IReadOnlyList<PlatformHealthDashboardItemDto>> GetPlatformHealthDashboardAsync(
        Guid tenantId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
