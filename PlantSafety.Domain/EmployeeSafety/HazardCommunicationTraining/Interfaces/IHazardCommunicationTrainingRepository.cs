// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.HazardCommunicationTraining
//  Layer:          Domain
//  File:           IHazardCommunicationTrainingRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the HazardCommunicationTraining aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.HazardCommunicationTraining.Interfaces;

/// <summary>Domain repository interface for the <see cref="HazardCommunicationTraining"/> aggregate root.</summary>
public interface IHazardCommunicationTrainingRepository
{
    /// <summary>Retrieves a HazardCommunicationTraining aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.EmployeeSafety.HazardCommunicationTraining.Aggregates.HazardCommunicationTraining?> GetByIdAsync(HazardCommunicationTrainingId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all HazardCommunicationTraining aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.EmployeeSafety.HazardCommunicationTraining.Aggregates.HazardCommunicationTraining>> ListAsync(HazardCommunicationTrainingSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new HazardCommunicationTraining aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.EmployeeSafety.HazardCommunicationTraining.Aggregates.HazardCommunicationTraining aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing HazardCommunicationTraining aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.EmployeeSafety.HazardCommunicationTraining.Aggregates.HazardCommunicationTraining aggregate, CancellationToken cancellationToken = default);
}