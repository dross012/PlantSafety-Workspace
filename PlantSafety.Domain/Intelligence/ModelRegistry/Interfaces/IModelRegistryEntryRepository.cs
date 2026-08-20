// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.ModelRegistry
//  Layer:          Domain
//  File:           IModelRegistryEntryRepository.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain (write-side) repository interface for the ModelRegistryEntry aggregate
//                  root. Ported (rebuilt) from the orphaned, dead
//                  Intelligence.Domain/ModelRegistry/Interfaces/IModelRegistryRepository.cs onto
//                  this module's own tenant-scoped GetByIdAsync/AddAsync/UpdateAsync(ETag)
//                  convention. Mirrors
//                  PlantSafety.Domain.Intelligence.DataLake.Interfaces.IDataLakeActivityRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Intelligence.ModelRegistry.ValueObjects;
using PlantSafety.Domain.Intelligence.ModelRegistry.ValueObjects.Ids;

namespace PlantSafety.Domain.Intelligence.ModelRegistry.Interfaces;

/// <summary>Domain repository interface for the ModelRegistryEntry aggregate root.</summary>
public interface IModelRegistryEntryRepository
{
    /// <summary>
    /// Retrieves a ModelRegistryEntry aggregate by its identity, scoped to
    /// <paramref name="tenantId"/>, or <see langword="null"/> if not found or if it belongs to a
    /// different tenant.
    /// </summary>
    Task<PlantSafety.Domain.Intelligence.ModelRegistry.Aggregates.ModelRegistryEntry?> GetByIdAsync(
        ModelRegistryEntryId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Returns the entry for the given model name and version, scoped to the tenant, or null when not found.</summary>
    Task<PlantSafety.Domain.Intelligence.ModelRegistry.Aggregates.ModelRegistryEntry?> GetByModelNameAndVersionAsync(
        string modelName, ModelVersion version, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Returns all entries currently in Approved status for the given model name, scoped to the tenant.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.Intelligence.ModelRegistry.Aggregates.ModelRegistryEntry>> GetApprovedByModelNameAsync(
        string modelName, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Persists a new ModelRegistryEntry aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.Intelligence.ModelRegistry.Aggregates.ModelRegistryEntry aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing ModelRegistryEntry aggregate, enforcing optimistic concurrency.</summary>
    /// <param name="aggregate">The mutated aggregate to persist.</param>
    /// <param name="eTag">
    /// The last-known concurrency token (from a prior read). A stale or mismatched value causes
    /// the persistence layer to reject the write as a concurrency conflict (zero rows affected).
    /// </param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task UpdateAsync(PlantSafety.Domain.Intelligence.ModelRegistry.Aggregates.ModelRegistryEntry aggregate, string? eTag, CancellationToken cancellationToken = default);
}
