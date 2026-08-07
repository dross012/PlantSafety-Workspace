// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Packaging
//  Layer:          Domain
//  File:           IPackagingSpecificationRepository.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface (write-side) for the PackagingSpecification aggregate root.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Packaging.Aggregates;
using PlantSafety.Domain.Compliance.PHMSA.Packaging.ValueObjects.Ids;

namespace PlantSafety.Domain.Compliance.PHMSA.Packaging.Interfaces;

/// <summary>Domain repository interface (write-side) for the <c>PackagingSpecification</c> aggregate root.</summary>
public interface IPackagingSpecificationRepository
{
    /// <summary>Retrieves a PackagingSpecification aggregate by its identity, scoped to the given tenant, or <see langword="null"/> if not found.</summary>
    Task<PackagingSpecification?> GetByIdAsync(PackagingSpecificationId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Persists a new PackagingSpecification aggregate.</summary>
    Task AddAsync(PackagingSpecification aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes to an existing PackagingSpecification aggregate, enforcing optimistic concurrency.
    /// </summary>
    /// <param name="aggregate">The mutated aggregate to persist.</param>
    /// <param name="eTag">The last-known concurrency token (from a prior read).</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task UpdateAsync(PackagingSpecification aggregate, string? eTag, CancellationToken cancellationToken = default);
}
