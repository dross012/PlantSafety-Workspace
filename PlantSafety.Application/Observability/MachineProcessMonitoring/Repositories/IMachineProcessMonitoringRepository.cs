// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the MachineProcessMonitoring read-model.
// SchemaVersion: 1

using PlantSafety.Application.Observability.MachineProcessMonitoring.Models;

namespace PlantSafety.Application.Observability.MachineProcessMonitoring.Repositories;

/// <summary>Read-only repository for MachineProcessMonitoring data. Implemented in the Infrastructure layer.</summary>
public interface IMachineProcessMonitoringRepository
{
    /// <summary>Returns MachineProcessMonitoring items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<MachineProcessMonitoringItemDto>> GetMachineProcessMonitoringAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}