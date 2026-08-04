// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.OperatingGeofence
//  Layer:          Domain
//  File:           IOperatingGeofenceRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the OperatingGeofence aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.OperatingGeofence.Interfaces;

/// <summary>Domain repository interface for the <see cref="Aggregates.OperatingGeofence"/> aggregate root.</summary>
public interface IOperatingGeofenceRepository
{
    /// <summary>Retrieves an OperatingGeofence aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<Aggregates.OperatingGeofence?> GetByIdAsync(OperatingGeofenceId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all OperatingGeofence aggregates matching the given specification.</summary>
    Task<IReadOnlyList<Aggregates.OperatingGeofence>> ListAsync(OperatingGeofenceSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new OperatingGeofence aggregate.</summary>
    Task AddAsync(Aggregates.OperatingGeofence aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing OperatingGeofence aggregate.</summary>
    Task UpdateAsync(Aggregates.OperatingGeofence aggregate, CancellationToken cancellationToken = default);
}
