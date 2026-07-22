// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the ActiveMonitoringAuditing read-model.
// SchemaVersion: 1

using PlantSafety.Application.ContractorSafety.ActiveMonitoringAuditing.Models;

namespace PlantSafety.Application.ContractorSafety.ActiveMonitoringAuditing.Repositories;

/// <summary>Read-only repository for ActiveMonitoringAuditing data. Implemented in the Infrastructure layer.</summary>
public interface IActiveMonitoringAuditingRepository
{
    /// <summary>Returns ActiveMonitoringAuditing items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<ActiveMonitoringAuditingItemDto>> GetActiveMonitoringAuditingItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}