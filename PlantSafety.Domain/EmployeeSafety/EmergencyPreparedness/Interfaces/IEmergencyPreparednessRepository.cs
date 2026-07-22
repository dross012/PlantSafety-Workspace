// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.EmergencyPreparedness
//  Layer:          Domain
//  File:           IEmergencyPreparednessRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the EmergencyPreparedness aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.EmergencyPreparedness.Interfaces;

/// <summary>Domain repository interface for the <see cref="EmergencyPreparedness"/> aggregate root.</summary>
public interface IEmergencyPreparednessRepository
{
    /// <summary>Retrieves a EmergencyPreparedness aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.EmployeeSafety.EmergencyPreparedness.Aggregates.EmergencyPreparedness?> GetByIdAsync(EmergencyPreparednessId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all EmergencyPreparedness aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.EmployeeSafety.EmergencyPreparedness.Aggregates.EmergencyPreparedness>> ListAsync(EmergencyPreparednessSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new EmergencyPreparedness aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.EmployeeSafety.EmergencyPreparedness.Aggregates.EmergencyPreparedness aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing EmergencyPreparedness aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.EmployeeSafety.EmergencyPreparedness.Aggregates.EmergencyPreparedness aggregate, CancellationToken cancellationToken = default);
}