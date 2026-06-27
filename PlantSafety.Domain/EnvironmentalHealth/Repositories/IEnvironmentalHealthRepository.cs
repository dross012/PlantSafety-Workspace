// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth
//  Layer:          Domain
//  File:           IEnvironmentalHealthRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the EnvironmentalHealth aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.Repositories;

/// <summary>Domain repository interface for the <see cref="EnvironmentalHealth"/> aggregate root.</summary>
public interface IEnvironmentalHealthRepository
{
    /// <summary>Retrieves a EnvironmentalHealth aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<EnvironmentalHealth?> GetByIdAsync(EnvironmentalHealthId id, CancellationToken cancellationToken = default);

    /// <summary>Persists a new EnvironmentalHealth aggregate.</summary>
    Task AddAsync(EnvironmentalHealth aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing EnvironmentalHealth aggregate.</summary>
    Task UpdateAsync(EnvironmentalHealth aggregate, CancellationToken cancellationToken = default);
}
