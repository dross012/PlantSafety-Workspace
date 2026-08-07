// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Classification
//  Layer:          Domain
//  File:           IHazmatMaterialRepository.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface (write-side) for the HazmatMaterial aggregate root.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Classification.Aggregates;
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects;
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects.Ids;

namespace PlantSafety.Domain.Compliance.PHMSA.Classification.Interfaces;

/// <summary>Domain repository interface (write-side) for the <c>HazmatMaterial</c> aggregate root.</summary>
public interface IHazmatMaterialRepository
{
    /// <summary>Retrieves a HazmatMaterial aggregate by its identity, scoped to the given tenant, or <see langword="null"/> if not found.</summary>
    Task<HazmatMaterial?> GetByIdAsync(HazmatMaterialId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Retrieves a HazmatMaterial aggregate by its UN number, scoped to the given tenant, or <see langword="null"/> if not found.</summary>
    Task<HazmatMaterial?> GetByUnNumberAsync(UNNumber unNumber, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Persists a new HazmatMaterial aggregate.</summary>
    Task AddAsync(HazmatMaterial aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes to an existing HazmatMaterial aggregate, enforcing optimistic concurrency.
    /// </summary>
    /// <param name="aggregate">The mutated aggregate to persist.</param>
    /// <param name="eTag">The last-known concurrency token (from a prior read).</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task UpdateAsync(HazmatMaterial aggregate, string? eTag, CancellationToken cancellationToken = default);
}
