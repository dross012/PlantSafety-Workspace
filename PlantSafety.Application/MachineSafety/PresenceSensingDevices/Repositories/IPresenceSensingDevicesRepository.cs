// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the PresenceSensingDevices read-model.
// SchemaVersion: 1

using PlantSafety.Application.MachineSafety.PresenceSensingDevices.Models;

namespace PlantSafety.Application.MachineSafety.PresenceSensingDevices.Repositories;

/// <summary>Read-only repository for PresenceSensingDevices data. Implemented in the Infrastructure layer.</summary>
public interface IPresenceSensingDevicesRepository
{
    /// <summary>Returns PresenceSensingDevices items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<PresenceSensingDevicesItemDto>> GetPresenceSensingDevicesItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}