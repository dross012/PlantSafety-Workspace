// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.TwoHandControls
//  Layer:          Domain
//  File:           ITwoHandControlsRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the TwoHandControls aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.TwoHandControls.Interfaces;

/// <summary>Domain repository interface for the <see cref="TwoHandControls"/> aggregate root.</summary>
public interface ITwoHandControlsRepository
{
    /// <summary>Retrieves a TwoHandControls aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.MachineSafety.TwoHandControls.Aggregates.TwoHandControls?> GetByIdAsync(TwoHandControlsId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all TwoHandControls aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.MachineSafety.TwoHandControls.Aggregates.TwoHandControls>> ListAsync(TwoHandControlsSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new TwoHandControls aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.MachineSafety.TwoHandControls.Aggregates.TwoHandControls aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing TwoHandControls aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.MachineSafety.TwoHandControls.Aggregates.TwoHandControls aggregate, CancellationToken cancellationToken = default);
}