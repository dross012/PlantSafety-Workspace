/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               IFeatureSetRepository.cs
    DESCRIPTION:        Repository contract for FeatureSet entities. FeatureSet is not an aggregate root
                        (it is owned by the Feature Engineering subsystem and published immutably), so this
                        interface does not extend IRepository<,>. The application layer uses this to check
                        for existing published versions and to persist newly computed feature sets.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using Intelligence.Domain.FeatureEngineering.Entities;
using Intelligence.Domain.FeatureEngineering.ValueObjects;

namespace Intelligence.Domain.FeatureEngineering.Interfaces;

/// <summary>Persistence contract for <see cref="FeatureSet"/> entities.</summary>
public interface IFeatureSetRepository
{
    /// <summary>Returns the feature set with the given identifier, or null when not found.</summary>
    Task<FeatureSet?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the most recently published feature set for the given name, or null when none exists.
    /// Used by the domain service to determine whether a new version supersedes the current one.
    /// </summary>
    Task<FeatureSet?> GetLatestPublishedAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>Returns all published feature sets for the given name, ordered by version descending.</summary>
    Task<IReadOnlyList<FeatureSet>> GetPublishedByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>Persists a new feature set. Committed when the Unit of Work completes.</summary>
    Task AddAsync(FeatureSet featureSet, CancellationToken cancellationToken = default);

    /// <summary>Marks an existing feature set as modified (e.g., after Publish()). Committed when the Unit of Work completes.</summary>
    Task UpdateAsync(FeatureSet featureSet, CancellationToken cancellationToken = default);
}
