// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PMScheduling
//  Layer:          Domain
//  File:           IPMSchedulingRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the PMScheduling aggregate root.
//                  Additive-only evolution.
//  UPDATED:        2026-07-30 -- GetByIdAsync now requires an explicit tenantId (Phase 4
//                  cross-tenant access remediation, mirrors IRiskRepository.GetByIdAsync /
//                  IIncidentRepository.GetByIdAsync): usp_PMScheduling_GetById previously filtered
//                  only by PMSchedulingId, so any caller who knew or guessed another tenant's
//                  PMSchedulingId could read (and, via UpdateAsync on the loaded aggregate, mutate)
//                  that tenant's record. The tenant check now happens at the DB boundary.
//  UPDATED:        2026-07-30 -- UpdateAsync now takes an optimistic-concurrency ETag (Phase 4
//                  concurrency remediation, mirrors IRiskRepository.UpdateAsync). Callers must
//                  supply the last-known ETag; a stale value causes ConcurrencyConflictException
//                  rather than a blind overwrite.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.PMScheduling.Interfaces;

/// <summary>Domain repository interface for the <see cref="PMScheduling"/> aggregate root.</summary>
public interface IPMSchedulingRepository
{
    /// <summary>
    /// Retrieves a PMScheduling aggregate by its identity, scoped to the given tenant, or
    /// <see langword="null"/> if not found or if the record belongs to a different tenant.
    /// </summary>
    Task<PlantSafety.Domain.Scheduling.PMScheduling.Aggregates.PMScheduling?> GetByIdAsync(PMSchedulingId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Returns all PMScheduling aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.Scheduling.PMScheduling.Aggregates.PMScheduling>> ListAsync(PMSchedulingSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new PMScheduling aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.Scheduling.PMScheduling.Aggregates.PMScheduling aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes to an existing PMScheduling aggregate, enforcing optimistic concurrency.
    /// </summary>
    /// <param name="aggregate">The mutated aggregate to persist.</param>
    /// <param name="eTag">
    /// The last-known concurrency token (from a prior read). A stale or mismatched value causes
    /// the persistence layer to reject the write as a concurrency conflict (zero rows affected).
    /// </param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task UpdateAsync(PlantSafety.Domain.Scheduling.PMScheduling.Aggregates.PMScheduling aggregate, string? eTag, CancellationToken cancellationToken = default);
}