// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Repository abstraction for the OperatingGeofence read-model.
// SchemaVersion: 1

using PlantSafety.Application.AutonomousVehicleSafety.OperatingGeofence.Models;

namespace PlantSafety.Application.AutonomousVehicleSafety.OperatingGeofence.Repositories;

/// <summary>Read-only repository for OperatingGeofence data. Implemented in the Infrastructure layer.</summary>
public interface IOperatingGeofenceRepository
{
    /// <summary>Returns geofences with at least one recorded violation for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<OperatingGeofenceItemDto>> GetActiveGeofenceViolationsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
