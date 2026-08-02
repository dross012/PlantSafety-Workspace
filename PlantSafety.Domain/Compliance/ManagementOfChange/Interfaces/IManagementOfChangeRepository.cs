// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.ManagementOfChange
//  Layer:          Domain
//  File:           IManagementOfChangeRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the ManagementOfChange aggregate root.
//                  Additive-only evolution.
//  UPDATED:        2026-08-02 -- Phase 4 full build-out: GetByIdAsync now takes a tenantId
//                  parameter (tenant-scoped from the start) and UpdateAsync now takes an
//                  optimistic-concurrency ETag, mirroring IPermitsRepository's identical
//                  precedent. This aggregate previously had no write path at all -- no
//                  Application-layer command ever called these methods -- so there is no
//                  pre-existing 2-arg call site to migrate.
// ============================================================================
namespace PlantSafety.Domain.Compliance.ManagementOfChange.Interfaces;

/// <summary>Domain repository interface for the <see cref="ManagementOfChange"/> aggregate root.</summary>
public interface IManagementOfChangeRepository
{
    /// <summary>Retrieves a ManagementOfChange aggregate by its identity, scoped to the given tenant, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.Compliance.ManagementOfChange.Aggregates.ManagementOfChange?> GetByIdAsync(ManagementOfChangeId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Returns all ManagementOfChange aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.Compliance.ManagementOfChange.Aggregates.ManagementOfChange>> ListAsync(ManagementOfChangeSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new ManagementOfChange aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.Compliance.ManagementOfChange.Aggregates.ManagementOfChange aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes to an existing ManagementOfChange aggregate, enforcing optimistic concurrency.
    /// </summary>
    /// <param name="aggregate">The mutated aggregate to persist.</param>
    /// <param name="eTag">
    /// The last-known concurrency token (from a prior read). A stale or mismatched value causes
    /// the persistence layer to reject the write as a concurrency conflict (zero rows affected).
    /// </param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task UpdateAsync(PlantSafety.Domain.Compliance.ManagementOfChange.Aggregates.ManagementOfChange aggregate, string? eTag, CancellationToken cancellationToken = default);
}
