// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the PPEBehavioralMonitoring read-model.
// SchemaVersion: 1

using PlantSafety.Application.Observability.PPEBehavioralMonitoring.Models;

namespace PlantSafety.Application.Observability.PPEBehavioralMonitoring.Repositories;

/// <summary>Read-only repository for PPEBehavioralMonitoring data. Implemented in the Infrastructure layer.</summary>
public interface IPPEBehavioralMonitoringRepository
{
    /// <summary>Returns PPEBehavioralMonitoring items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<PPEBehavioralMonitoringItemDto>> GetPPEBehavioralMonitoringAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}