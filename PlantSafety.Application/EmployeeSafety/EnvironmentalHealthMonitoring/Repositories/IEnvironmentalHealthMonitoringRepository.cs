// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the EnvironmentalHealthMonitoring read-model.
// SchemaVersion: 1

using PlantSafety.Application.EmployeeSafety.EnvironmentalHealthMonitoring.Models;

namespace PlantSafety.Application.EmployeeSafety.EnvironmentalHealthMonitoring.Repositories;

/// <summary>Read-only repository for EnvironmentalHealthMonitoring data. Implemented in the Infrastructure layer.</summary>
public interface IEnvironmentalHealthMonitoringRepository
{
    /// <summary>Returns EnvironmentalHealthMonitoring items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<EnvironmentalHealthMonitoringItemDto>> GetEnvironmentalHealthMonitoringItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}