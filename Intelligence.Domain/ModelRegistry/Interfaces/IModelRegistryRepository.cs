/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               IModelRegistryRepository.cs
    DESCRIPTION:        Repository contract for ModelRegistryEntry. Note: ModelRegistryEntry is an Entity,
                        not an AggregateRoot, so this interface does not extend IRepository<,> directly.
    SCHEMA VERSION:     2026.1  |  EVOLUTION: Additive-only.
==========================================================================================================================*/

using Intelligence.Domain.Shared.Interfaces;
using Intelligence.Domain.ModelRegistry.Entities;
using Intelligence.Domain.ModelRegistry.ValueObjects;

namespace Intelligence.Domain.ModelRegistry.Interfaces;

/// <summary>Persistence contract for <see cref="ModelRegistryEntry"/> aggregates.</summary>
public interface IModelRegistryRepository : IRepository<ModelRegistryEntry, Guid>
{
    /// <summary>Returns the entry for the given model name and version, or null when not found.</summary>
    Task<ModelRegistryEntry?> GetByVersionAsync(string modelName, ModelVersion version, CancellationToken cancellationToken = default);

    /// <summary>Returns all entries for the given model name, ordered by registration date descending.</summary>
    Task<IReadOnlyList<ModelRegistryEntry>> GetByModelNameAsync(string modelName, CancellationToken cancellationToken = default);

    /// <summary>Returns all entries that are currently in Approved status.</summary>
    Task<IReadOnlyList<ModelRegistryEntry>> GetApprovedModelsAsync(CancellationToken cancellationToken = default);
}
