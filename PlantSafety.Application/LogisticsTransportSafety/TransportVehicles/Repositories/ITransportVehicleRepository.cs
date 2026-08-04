// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Repository abstraction for the TransportVehicle read-model.
// SchemaVersion: 1

using PlantSafety.Application.LogisticsTransportSafety.TransportVehicles.Models;

namespace PlantSafety.Application.LogisticsTransportSafety.TransportVehicles.Repositories;

/// <summary>Read-only repository for TransportVehicle data. Implemented in the Infrastructure layer.</summary>
public interface ITransportVehicleRepository
{
    /// <summary>Returns a single TransportVehicle item by identity, or null if not found.</summary>
    Task<TransportVehicleItemDto?> GetTransportVehicleByIdAsync(
        Guid tenantId,
        Guid transportVehicleId,
        CancellationToken cancellationToken);
}
