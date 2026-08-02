// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Compliance
//  Layer:          Domain
//  File:           IComplianceRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the Compliance aggregate root.
//                  Additive-only evolution.
//  UPDATED:        2026-08-02 -- Phase 4 full build-out: GetByIdAsync now takes a tenantId
//                  parameter (tenant-scoped from the start) and UpdateAsync now takes an
//                  optimistic-concurrency ETag, mirroring IPermitsRepository's identical retrofit.
//                  Every reference to the aggregate type below is fully qualified
//                  (PlantSafety.Domain.Compliance.Compliance.Aggregates.Compliance) -- the bare
//                  simple name "Compliance" collides with both this file's own namespace segment
//                  and the parent "Compliance" module namespace.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Compliance.Interfaces;

/// <summary>Domain repository interface for the <see cref="Compliance"/> aggregate root.</summary>
public interface IComplianceRepository
{
    /// <summary>Retrieves a Compliance aggregate by its identity, scoped to the given tenant, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.Compliance.Compliance.Aggregates.Compliance?> GetByIdAsync(ComplianceId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Returns all Compliance aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.Compliance.Compliance.Aggregates.Compliance>> ListAsync(ComplianceSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new Compliance aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.Compliance.Compliance.Aggregates.Compliance aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes to an existing Compliance aggregate, enforcing optimistic concurrency.
    /// </summary>
    /// <param name="aggregate">The mutated aggregate to persist.</param>
    /// <param name="eTag">
    /// The last-known concurrency token (from a prior read). A stale or mismatched value causes
    /// the persistence layer to reject the write as a concurrency conflict (zero rows affected).
    /// </param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task UpdateAsync(PlantSafety.Domain.Compliance.Compliance.Aggregates.Compliance aggregate, string? eTag, CancellationToken cancellationToken = default);
}
