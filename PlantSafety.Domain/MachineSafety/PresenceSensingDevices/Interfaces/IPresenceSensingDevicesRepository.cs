// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.PresenceSensingDevices
//  Layer:          Domain
//  File:           IPresenceSensingDevicesRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the PresenceSensingDevices aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.PresenceSensingDevices.Interfaces;

/// <summary>Domain repository interface for the <see cref="PresenceSensingDevices"/> aggregate root.</summary>
public interface IPresenceSensingDevicesRepository
{
    /// <summary>Retrieves a PresenceSensingDevices aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.MachineSafety.PresenceSensingDevices.Aggregates.PresenceSensingDevices?> GetByIdAsync(PresenceSensingDevicesId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all PresenceSensingDevices aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.MachineSafety.PresenceSensingDevices.Aggregates.PresenceSensingDevices>> ListAsync(PresenceSensingDevicesSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new PresenceSensingDevices aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.MachineSafety.PresenceSensingDevices.Aggregates.PresenceSensingDevices aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing PresenceSensingDevices aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.MachineSafety.PresenceSensingDevices.Aggregates.PresenceSensingDevices aggregate, CancellationToken cancellationToken = default);
}