// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability
//  Layer:          Domain
//  File:           ISustainabilityRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the Sustainability aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Sustainability.Repositories;

/// <summary>Domain repository interface for the <see cref="Sustainability"/> aggregate root.</summary>
public interface ISustainabilityRepository
{
    /// <summary>Retrieves a Sustainability aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.Sustainability.Aggregates.Sustainability?> GetByIdAsync(SustainabilityId id, CancellationToken cancellationToken = default);

    /// <summary>Persists a new Sustainability aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.Sustainability.Aggregates.Sustainability aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing Sustainability aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.Sustainability.Aggregates.Sustainability aggregate, CancellationToken cancellationToken = default);
}
