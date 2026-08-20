// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.FeatureEngineering
//  Layer:          Domain
//  File:           IFeatureSetRepository.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain (write-side) repository interface for the FeatureSet entity. Ported
//                  (rebuilt) from the orphaned, dead
//                  Intelligence.Domain/FeatureEngineering/Interfaces/IFeatureSetRepository.cs onto
//                  this module's own tenant-scoped GetByIdAsync/AddAsync/UpdateAsync(ETag)
//                  convention. FeatureSet is an Entity, not an aggregate root of its own (it is
//                  owned by the Feature Engineering subsystem and published immutably), matching
//                  the orphan's original documented design decision. Mirrors
//                  PlantSafety.Domain.Intelligence.DataLake.Interfaces.IDataLakeActivityRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Intelligence.FeatureEngineering.ValueObjects.Ids;

namespace PlantSafety.Domain.Intelligence.FeatureEngineering.Interfaces;

/// <summary>Persistence contract for <see cref="Entities.FeatureSet"/> entities.</summary>
public interface IFeatureSetRepository
{
    /// <summary>
    /// Retrieves a FeatureSet by its identity, scoped to <paramref name="tenantId"/>, or
    /// <see langword="null"/> if not found or if it belongs to a different tenant.
    /// </summary>
    Task<PlantSafety.Domain.Intelligence.FeatureEngineering.Entities.FeatureSet?> GetByIdAsync(
        FeatureSetId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the most recently published feature set for the given name, scoped to the tenant,
    /// or <see langword="null"/> when none exists. Used by
    /// <see cref="Services.FeatureEngineeringDomainService"/> to determine whether a new version
    /// supersedes the current one.
    /// </summary>
    Task<PlantSafety.Domain.Intelligence.FeatureEngineering.Entities.FeatureSet?> GetLatestPublishedAsync(
        string name, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Returns all published feature sets for the given name, scoped to the tenant, ordered by version descending.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.Intelligence.FeatureEngineering.Entities.FeatureSet>> GetPublishedByNameAsync(
        string name, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Persists a new feature set.</summary>
    Task AddAsync(PlantSafety.Domain.Intelligence.FeatureEngineering.Entities.FeatureSet featureSet, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing feature set (e.g., after AddFeature/Publish), enforcing optimistic concurrency.</summary>
    /// <param name="featureSet">The mutated feature set to persist.</param>
    /// <param name="eTag">The last-known concurrency token. A stale or mismatched value causes a concurrency-conflict rejection.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task UpdateAsync(PlantSafety.Domain.Intelligence.FeatureEngineering.Entities.FeatureSet featureSet, string? eTag, CancellationToken cancellationToken = default);
}
