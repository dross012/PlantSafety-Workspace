// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability
//  Layer:          Domain
//  File:           ISustainabilityRepository.cs
//  Created:        2026-06-17
//  Updated:        2026-08-02 -- Phase 4 full build-out: added tenant-scoping to GetByIdAsync and
//                  optimistic-concurrency ETag support to UpdateAsync.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the Sustainability aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Sustainability.Repositories;

/// <summary>Domain repository interface for the <see cref="Sustainability"/> aggregate root.</summary>
public interface ISustainabilityRepository
{
    /// <summary>Retrieves a Sustainability aggregate by its identity, scoped to the given tenant, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.Sustainability.Aggregates.Sustainability?> GetByIdAsync(SustainabilityId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Persists a new Sustainability aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.Sustainability.Aggregates.Sustainability aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing Sustainability aggregate, enforcing optimistic concurrency via <paramref name="eTag"/>.</summary>
    Task UpdateAsync(PlantSafety.Domain.Sustainability.Aggregates.Sustainability aggregate, string? eTag, CancellationToken cancellationToken = default);
}
