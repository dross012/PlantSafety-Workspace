// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-02
// FILE DESCRIPTION: Repository abstraction for the Audit BI dashboard read-model.
// SchemaVersion: 1

using PlantSafety.Application.Analytics.Dashboard.AuditDashboard.Models;

namespace PlantSafety.Application.Analytics.Dashboard.AuditDashboard.Repositories;

/// <summary>Read-only repository for Audit dashboard KPI data. Implemented in the Infrastructure layer.</summary>
public interface IAuditDashboardRepository
{
    /// <summary>Returns Audit dashboard KPI items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<AuditDashboardItemDto>> GetAuditDashboardAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
