// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-20
// FILE DESCRIPTION: Repository abstraction for the EnvironmentalMonitoring read-model.
// SchemaVersion: 1

using PlantSafety.Application.Operational.EnvironmentalMonitoring.Models;

namespace PlantSafety.Application.Operational.EnvironmentalMonitoring.Repositories;

/// <summary>Read-only repository for EnvironmentalMonitoring data. Implemented in the Infrastructure layer.</summary>
public interface IEnvironmentalMonitoringRepository
{
    /// <summary>Returns EnvironmentalMonitoring items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<EnvironmentalMonitoringItemDto>> GetEnvironmentalMonitoringAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}