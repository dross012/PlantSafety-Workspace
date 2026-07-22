// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the EnvironmentalAssetMonitoring read-model.
// SchemaVersion: 1

using PlantSafety.Application.Observability.EnvironmentalAssetMonitoring.Models;

namespace PlantSafety.Application.Observability.EnvironmentalAssetMonitoring.Repositories;

/// <summary>Read-only repository for EnvironmentalAssetMonitoring data. Implemented in the Infrastructure layer.</summary>
public interface IEnvironmentalAssetMonitoringRepository
{
    /// <summary>Returns EnvironmentalAssetMonitoring items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<EnvironmentalAssetMonitoringItemDto>> GetEnvironmentalAssetMonitoringAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}