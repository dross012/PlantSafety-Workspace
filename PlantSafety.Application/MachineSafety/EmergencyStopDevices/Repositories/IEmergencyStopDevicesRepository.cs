// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the EmergencyStopDevices read-model.
// SchemaVersion: 1

using PlantSafety.Application.MachineSafety.EmergencyStopDevices.Models;

namespace PlantSafety.Application.MachineSafety.EmergencyStopDevices.Repositories;

/// <summary>Read-only repository for EmergencyStopDevices data. Implemented in the Infrastructure layer.</summary>
public interface IEmergencyStopDevicesRepository
{
    /// <summary>Returns EmergencyStopDevices items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<EmergencyStopDevicesItemDto>> GetEmergencyStopDevicesItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}