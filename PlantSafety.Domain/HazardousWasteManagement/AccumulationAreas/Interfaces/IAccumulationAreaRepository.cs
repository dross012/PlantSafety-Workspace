// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.AccumulationAreas
//  Layer:          Domain
//  File:           IAccumulationAreaRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain repository interface for the AccumulationArea aggregate root.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.AccumulationAreas.Specifications;
using PlantSafety.Domain.HazardousWasteManagement.AccumulationAreas.ValueObjects.Ids;

namespace PlantSafety.Domain.HazardousWasteManagement.AccumulationAreas.Interfaces;

/// <summary>Domain repository interface for the AccumulationArea aggregate root.</summary>
public interface IAccumulationAreaRepository
{
    /// <summary>Retrieves an AccumulationArea aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.HazardousWasteManagement.AccumulationAreas.Aggregates.AccumulationArea?> GetByIdAsync(AccumulationAreaId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all AccumulationArea aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.HazardousWasteManagement.AccumulationAreas.Aggregates.AccumulationArea>> ListAsync(AccumulationAreaSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new AccumulationArea aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.HazardousWasteManagement.AccumulationAreas.Aggregates.AccumulationArea aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing AccumulationArea aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.HazardousWasteManagement.AccumulationAreas.Aggregates.AccumulationArea aggregate, CancellationToken cancellationToken = default);
}
