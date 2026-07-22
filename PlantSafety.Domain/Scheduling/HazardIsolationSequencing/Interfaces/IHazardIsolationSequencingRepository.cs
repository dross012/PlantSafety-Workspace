// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.HazardIsolationSequencing
//  Layer:          Domain
//  File:           IHazardIsolationSequencingRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the HazardIsolationSequencing aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.HazardIsolationSequencing.Interfaces;

/// <summary>Domain repository interface for the <see cref="HazardIsolationSequencing"/> aggregate root.</summary>
public interface IHazardIsolationSequencingRepository
{
    /// <summary>Retrieves a HazardIsolationSequencing aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.Scheduling.HazardIsolationSequencing.Aggregates.HazardIsolationSequencing?> GetByIdAsync(HazardIsolationSequencingId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all HazardIsolationSequencing aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.Scheduling.HazardIsolationSequencing.Aggregates.HazardIsolationSequencing>> ListAsync(HazardIsolationSequencingSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new HazardIsolationSequencing aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.Scheduling.HazardIsolationSequencing.Aggregates.HazardIsolationSequencing aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing HazardIsolationSequencing aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.Scheduling.HazardIsolationSequencing.Aggregates.HazardIsolationSequencing aggregate, CancellationToken cancellationToken = default);
}