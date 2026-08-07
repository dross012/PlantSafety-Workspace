// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Placarding
//  Layer:          Domain
//  File:           IHazmatContainerRepository.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the HazmatContainer aggregate root.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Placarding.Aggregates;
using PlantSafety.Domain.Compliance.PHMSA.Placarding.ValueObjects.Ids;

namespace PlantSafety.Domain.Compliance.PHMSA.Placarding.Interfaces;

/// <summary>Domain repository interface for the <see cref="HazmatContainer"/> aggregate root.</summary>
public interface IHazmatContainerRepository
{
    /// <summary>Retrieves a HazmatContainer aggregate by its identity, scoped to the given tenant, or <see langword="null"/> if not found.</summary>
    Task<HazmatContainer?> GetByIdAsync(HazmatContainerId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Persists a new HazmatContainer aggregate.</summary>
    Task AddAsync(HazmatContainer aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes to an existing HazmatContainer aggregate, enforcing optimistic concurrency.
    /// </summary>
    /// <param name="aggregate">The mutated aggregate to persist.</param>
    /// <param name="eTag">
    /// The last-known concurrency token (from a prior read). A stale or mismatched value causes
    /// the persistence layer to reject the write as a concurrency conflict (zero rows affected).
    /// </param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task UpdateAsync(HazmatContainer aggregate, string? eTag, CancellationToken cancellationToken = default);
}
