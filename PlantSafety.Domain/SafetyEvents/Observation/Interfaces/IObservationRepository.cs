// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.Observation
//  Layer:          Domain
//  File:           IObservationRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the Observation aggregate root.
//                  Additive-only evolution.
//  UPDATED:        2026-08-02 -- Phase 4 full build-out: GetByIdAsync now takes a tenantId
//                  parameter (tenant-scoped from the start) and UpdateAsync now takes an
//                  optimistic-concurrency ETag. Mirrors
//                  PlantSafety.Domain.Compliance.Permits.Interfaces.IPermitsRepository's
//                  identical retrofit. This aggregate previously had no write path at all -- no
//                  Application-layer command ever called these methods -- so there is no
//                  pre-existing 2-arg call site to migrate.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.Observation.Interfaces;

/// <summary>Domain repository interface for the <see cref="Observation"/> aggregate root.</summary>
public interface IObservationRepository
{
    /// <summary>Retrieves a Observation aggregate by its identity, scoped to the given tenant, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.SafetyEvents.Observation.Aggregates.Observation?> GetByIdAsync(ObservationId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Returns all Observation aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.SafetyEvents.Observation.Aggregates.Observation>> ListAsync(ObservationSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new Observation aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.SafetyEvents.Observation.Aggregates.Observation aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes to an existing Observation aggregate, enforcing optimistic concurrency.
    /// </summary>
    /// <param name="aggregate">The mutated aggregate to persist.</param>
    /// <param name="eTag">
    /// The last-known concurrency token (from a prior read). A stale or mismatched value causes
    /// the persistence layer to reject the write as a concurrency conflict (zero rows affected).
    /// </param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task UpdateAsync(PlantSafety.Domain.SafetyEvents.Observation.Aggregates.Observation aggregate, string? eTag, CancellationToken cancellationToken = default);
}
