// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPECategory
//  Layer:          Domain
//  File:           IPPECategoryRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the PPECategory aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPECategory.Interfaces;

/// <summary>Domain repository interface for the <see cref="PPECategory"/> aggregate root.</summary>
public interface IPPECategoryRepository
{
    /// <summary>Retrieves a PPECategory aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.PPE.PPECategory.Aggregates.PPECategory?> GetByIdAsync(PPECategoryId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all PPECategory aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.PPE.PPECategory.Aggregates.PPECategory>> ListAsync(PPECategorySpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new PPECategory aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.PPE.PPECategory.Aggregates.PPECategory aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing PPECategory aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.PPE.PPECategory.Aggregates.PPECategory aggregate, CancellationToken cancellationToken = default);
}