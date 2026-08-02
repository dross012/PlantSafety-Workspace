// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.HazardIsolationSequencing
//  Layer:          Domain
//  File:           IHazardIsolationSequencingRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the HazardIsolationSequencing aggregate root.
//                  Additive-only evolution.
//  UPDATED:        2026-07-30 -- Phase 4 full build-out: GetByIdAsync now takes a tenantId
//                  parameter (tenant-scoped from the start, mirroring
//                  IAccidentRepository.GetByIdAsync(AccidentId, Guid, CancellationToken)) and
//                  UpdateAsync now takes an optimistic-concurrency ETag (mirroring
//                  IPermitsRepository.UpdateAsync(Permits, string?, CancellationToken)). This
//                  aggregate previously had no write path at all, so there is no pre-existing
//                  2-arg call site to migrate.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.HazardIsolationSequencing.Interfaces;

/// <summary>Domain repository interface for the <see cref="HazardIsolationSequencing"/> aggregate root.</summary>
public interface IHazardIsolationSequencingRepository
{
    /// <summary>Retrieves a HazardIsolationSequencing aggregate by its identity, scoped to the given tenant, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.Scheduling.HazardIsolationSequencing.Aggregates.HazardIsolationSequencing?> GetByIdAsync(HazardIsolationSequencingId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Returns all HazardIsolationSequencing aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.Scheduling.HazardIsolationSequencing.Aggregates.HazardIsolationSequencing>> ListAsync(HazardIsolationSequencingSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new HazardIsolationSequencing aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.Scheduling.HazardIsolationSequencing.Aggregates.HazardIsolationSequencing aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes to an existing HazardIsolationSequencing aggregate, enforcing optimistic concurrency.
    /// </summary>
    /// <param name="aggregate">The mutated aggregate to persist.</param>
    /// <param name="eTag">
    /// The last-known concurrency token (from a prior read). A stale or mismatched value causes
    /// the persistence layer to reject the write as a concurrency conflict (zero rows affected).
    /// </param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task UpdateAsync(PlantSafety.Domain.Scheduling.HazardIsolationSequencing.Aggregates.HazardIsolationSequencing aggregate, string? eTag, CancellationToken cancellationToken = default);
}