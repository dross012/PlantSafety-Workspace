// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.AutonomousVehicleIncident
//  Layer:          Domain
//  File:           IAutonomousVehicleIncidentRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the AutonomousVehicleIncident aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleIncident.Interfaces;

/// <summary>Domain repository interface for the <see cref="Aggregates.AutonomousVehicleIncident"/> aggregate root.</summary>
public interface IAutonomousVehicleIncidentRepository
{
    /// <summary>Retrieves an AutonomousVehicleIncident aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<Aggregates.AutonomousVehicleIncident?> GetByIdAsync(AutonomousVehicleIncidentId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all AutonomousVehicleIncident aggregates matching the given specification.</summary>
    Task<IReadOnlyList<Aggregates.AutonomousVehicleIncident>> ListAsync(AutonomousVehicleIncidentSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new AutonomousVehicleIncident aggregate.</summary>
    Task AddAsync(Aggregates.AutonomousVehicleIncident aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing AutonomousVehicleIncident aggregate.</summary>
    Task UpdateAsync(Aggregates.AutonomousVehicleIncident aggregate, CancellationToken cancellationToken = default);
}
