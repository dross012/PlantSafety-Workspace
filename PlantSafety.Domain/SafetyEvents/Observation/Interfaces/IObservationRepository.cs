// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.Observation
//  Layer:          Domain
//  File:           IObservationRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the Observation aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.Observation.Interfaces;

/// <summary>Domain repository interface for the <see cref="Observation"/> aggregate root.</summary>
public interface IObservationRepository
{
    /// <summary>Retrieves a Observation aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<Observation?> GetByIdAsync(ObservationId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all Observation aggregates matching the given specification.</summary>
    Task<IReadOnlyList<Observation>> ListAsync(ObservationSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new Observation aggregate.</summary>
    Task AddAsync(Observation aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing Observation aggregate.</summary>
    Task UpdateAsync(Observation aggregate, CancellationToken cancellationToken = default);
}
