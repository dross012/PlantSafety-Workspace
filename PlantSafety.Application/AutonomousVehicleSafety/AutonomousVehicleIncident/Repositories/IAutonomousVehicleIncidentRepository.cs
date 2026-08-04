// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Repository abstraction for the AutonomousVehicleIncident read-model.
// SchemaVersion: 1

using PlantSafety.Application.AutonomousVehicleSafety.AutonomousVehicleIncident.Models;

namespace PlantSafety.Application.AutonomousVehicleSafety.AutonomousVehicleIncident.Repositories;

/// <summary>Read-only repository for AutonomousVehicleIncident data. Implemented in the Infrastructure layer.</summary>
public interface IAutonomousVehicleIncidentRepository
{
    /// <summary>Returns the full incident history for the given autonomous vehicle asset, most recent first.</summary>
    Task<IReadOnlyList<AutonomousVehicleIncidentItemDto>> GetIncidentHistoryByAssetIdAsync(
        Guid tenantId,
        Guid autonomousVehicleAssetId,
        CancellationToken cancellationToken);
}
