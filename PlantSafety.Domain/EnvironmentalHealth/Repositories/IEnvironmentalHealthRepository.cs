// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth
//  Layer:          Domain
//  File:           IEnvironmentalHealthRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the EnvironmentalHealth aggregate root.
//                  Additive-only evolution.
//  UPDATED:        2026-08-02 -- Phase 4 full build-out: GetByIdAsync now takes a tenantId
//                  parameter (tenant-scoped from the start) and UpdateAsync now takes an
//                  optimistic-concurrency ETag, mirroring IAuditingRepository's identical
//                  retrofit. This aggregate previously had no write path at all -- no
//                  Application-layer command ever called these methods -- so there is no
//                  pre-existing 2-arg call site to migrate.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.Repositories;

/// <summary>Domain repository interface for the <see cref="EnvironmentalHealth"/> aggregate root.</summary>
public interface IEnvironmentalHealthRepository
{
    /// <summary>Retrieves a EnvironmentalHealth aggregate by its identity, scoped to the given tenant, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.EnvironmentalHealth.Aggregates.EnvironmentalHealth?> GetByIdAsync(EnvironmentalHealthId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Persists a new EnvironmentalHealth aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.EnvironmentalHealth.Aggregates.EnvironmentalHealth aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes to an existing EnvironmentalHealth aggregate, enforcing optimistic concurrency.
    /// </summary>
    /// <param name="aggregate">The mutated aggregate to persist.</param>
    /// <param name="eTag">
    /// The last-known concurrency token (from a prior read). A stale or mismatched value causes
    /// the persistence layer to reject the write as a concurrency conflict (zero rows affected).
    /// </param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task UpdateAsync(PlantSafety.Domain.EnvironmentalHealth.Aggregates.EnvironmentalHealth aggregate, string? eTag, CancellationToken cancellationToken = default);
}
