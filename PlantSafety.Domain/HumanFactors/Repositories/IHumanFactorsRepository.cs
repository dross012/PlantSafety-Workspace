// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors
//  Layer:          Domain
//  File:           IHumanFactorsRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the HumanFactors aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HumanFactors.Repositories;

/// <summary>Domain repository interface for the <see cref="HumanFactors"/> aggregate root.</summary>
public interface IHumanFactorsRepository
{
    /// <summary>Retrieves a HumanFactors aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.HumanFactors.Aggregates.HumanFactors?> GetByIdAsync(HumanFactorsId id, CancellationToken cancellationToken = default);

    /// <summary>Persists a new HumanFactors aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.HumanFactors.Aggregates.HumanFactors aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing HumanFactors aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.HumanFactors.Aggregates.HumanFactors aggregate, CancellationToken cancellationToken = default);
}
