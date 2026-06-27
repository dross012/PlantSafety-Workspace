// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Training
//  Layer:          Domain
//  File:           ITrainingRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the Training aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Training.Interfaces;

/// <summary>Domain repository interface for the <see cref="Training"/> aggregate root.</summary>
public interface ITrainingRepository
{
    /// <summary>Retrieves a Training aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<Training?> GetByIdAsync(TrainingId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all Training aggregates matching the given specification.</summary>
    Task<IReadOnlyList<Training>> ListAsync(TrainingSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new Training aggregate.</summary>
    Task AddAsync(Training aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing Training aggregate.</summary>
    Task UpdateAsync(Training aggregate, CancellationToken cancellationToken = default);
}
