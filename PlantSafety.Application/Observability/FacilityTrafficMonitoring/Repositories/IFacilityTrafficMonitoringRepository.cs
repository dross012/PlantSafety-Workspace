// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the FacilityTrafficMonitoring read-model.
// SchemaVersion: 1

using PlantSafety.Application.Observability.FacilityTrafficMonitoring.Models;

namespace PlantSafety.Application.Observability.FacilityTrafficMonitoring.Repositories;

/// <summary>Read-only repository for FacilityTrafficMonitoring data. Implemented in the Infrastructure layer.</summary>
public interface IFacilityTrafficMonitoringRepository
{
    /// <summary>Returns FacilityTrafficMonitoring items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<FacilityTrafficMonitoringItemDto>> GetFacilityTrafficMonitoringAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}