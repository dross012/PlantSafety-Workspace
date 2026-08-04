// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.AutonomousVehicleAsset
//  Layer:          Domain
//  File:           IAutonomousVehicleAssetRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the AutonomousVehicleAsset aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleAsset.Interfaces;

/// <summary>Domain repository interface for the <see cref="Aggregates.AutonomousVehicleAsset"/> aggregate root.</summary>
public interface IAutonomousVehicleAssetRepository
{
    /// <summary>Retrieves an AutonomousVehicleAsset aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<Aggregates.AutonomousVehicleAsset?> GetByIdAsync(AutonomousVehicleAssetId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all AutonomousVehicleAsset aggregates matching the given specification.</summary>
    Task<IReadOnlyList<Aggregates.AutonomousVehicleAsset>> ListAsync(AutonomousVehicleAssetSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new AutonomousVehicleAsset aggregate.</summary>
    Task AddAsync(Aggregates.AutonomousVehicleAsset aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing AutonomousVehicleAsset aggregate.</summary>
    Task UpdateAsync(Aggregates.AutonomousVehicleAsset aggregate, CancellationToken cancellationToken = default);
}
