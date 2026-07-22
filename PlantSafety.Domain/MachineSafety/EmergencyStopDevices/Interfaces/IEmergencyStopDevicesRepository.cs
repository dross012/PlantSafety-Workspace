// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.EmergencyStopDevices
//  Layer:          Domain
//  File:           IEmergencyStopDevicesRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the EmergencyStopDevices aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.EmergencyStopDevices.Interfaces;

/// <summary>Domain repository interface for the <see cref="EmergencyStopDevices"/> aggregate root.</summary>
public interface IEmergencyStopDevicesRepository
{
    /// <summary>Retrieves a EmergencyStopDevices aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.MachineSafety.EmergencyStopDevices.Aggregates.EmergencyStopDevices?> GetByIdAsync(EmergencyStopDevicesId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all EmergencyStopDevices aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.MachineSafety.EmergencyStopDevices.Aggregates.EmergencyStopDevices>> ListAsync(EmergencyStopDevicesSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new EmergencyStopDevices aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.MachineSafety.EmergencyStopDevices.Aggregates.EmergencyStopDevices aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing EmergencyStopDevices aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.MachineSafety.EmergencyStopDevices.Aggregates.EmergencyStopDevices aggregate, CancellationToken cancellationToken = default);
}