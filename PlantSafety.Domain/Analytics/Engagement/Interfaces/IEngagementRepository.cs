// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Engagement
//  Layer:          Domain
//  File:           IEngagementRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the Engagement aggregate root.
//                  Additive-only evolution.
//  UPDATED:        2026-08-02 -- Phase 4 full build-out: GetByIdAsync now takes a tenantId
//                  parameter (tenant-scoped from the start, mirroring
//                  IPermitsRepository.GetByIdAsync(PermitsId, Guid, CancellationToken)) and
//                  UpdateAsync now takes an optimistic-concurrency ETag. This aggregate previously
//                  had no write path at all -- confirmed zero files anywhere under
//                  PlantSafety.Application\Analytics\Engagement referenced this aggregate -- so
//                  there is no pre-existing 2-arg call site to migrate.
// ============================================================================
namespace PlantSafety.Domain.Analytics.Engagement.Interfaces;

/// <summary>Domain repository interface for the <see cref="Engagement"/> aggregate root.</summary>
public interface IEngagementRepository
{
    /// <summary>Retrieves a Engagement aggregate by its identity, scoped to the given tenant, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.Analytics.Engagement.Aggregates.Engagement?> GetByIdAsync(EngagementId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Returns all Engagement aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.Analytics.Engagement.Aggregates.Engagement>> ListAsync(EngagementSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new Engagement aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.Analytics.Engagement.Aggregates.Engagement aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes to an existing Engagement aggregate, enforcing optimistic concurrency.
    /// </summary>
    /// <param name="aggregate">The mutated aggregate to persist.</param>
    /// <param name="eTag">
    /// The last-known concurrency token (from a prior read). A stale or mismatched value causes
    /// the persistence layer to reject the write as a concurrency conflict (zero rows affected).
    /// </param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task UpdateAsync(PlantSafety.Domain.Analytics.Engagement.Aggregates.Engagement aggregate, string? eTag, CancellationToken cancellationToken = default);
}
