// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Repository abstraction for the AutonomousVehicleAsset read-model.
// SchemaVersion: 1

using PlantSafety.Application.AutonomousVehicleSafety.AutonomousVehicleAsset.Models;

namespace PlantSafety.Application.AutonomousVehicleSafety.AutonomousVehicleAsset.Repositories;

/// <summary>Read-only repository for AutonomousVehicleAsset data. Implemented in the Infrastructure layer.</summary>
public interface IAutonomousVehicleAssetRepository
{
    /// <summary>Returns a single AutonomousVehicleAsset item by identifier, or <see langword="null"/> if not found.</summary>
    Task<AutonomousVehicleAssetItemDto?> GetAutonomousVehicleAssetByIdAsync(
        Guid tenantId,
        Guid autonomousVehicleAssetId,
        CancellationToken cancellationToken);
}
