// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Certification
//  Layer:          Domain
//  File:           ICertificationRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the Certification aggregate root.
//                  Additive-only evolution.
//  UPDATED:        2026-08-02 -- Phase 4 full build-out: GetByIdAsync now takes a tenantId
//                  parameter (tenant-scoped from the start) and UpdateAsync now takes an
//                  optimistic-concurrency ETag, mirroring IPermitsRepository's identical retrofit.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Certification.Interfaces;

/// <summary>Domain repository interface for the <see cref="Certification"/> aggregate root.</summary>
public interface ICertificationRepository
{
    /// <summary>Retrieves a Certification aggregate by its identity, scoped to the given tenant, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.Compliance.Certification.Aggregates.Certification?> GetByIdAsync(CertificationId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Returns all Certification aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.Compliance.Certification.Aggregates.Certification>> ListAsync(CertificationSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new Certification aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.Compliance.Certification.Aggregates.Certification aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes to an existing Certification aggregate, enforcing optimistic concurrency.
    /// </summary>
    /// <param name="aggregate">The mutated aggregate to persist.</param>
    /// <param name="eTag">
    /// The last-known concurrency token (from a prior read). A stale or mismatched value causes
    /// the persistence layer to reject the write as a concurrency conflict (zero rows affected).
    /// </param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task UpdateAsync(PlantSafety.Domain.Compliance.Certification.Aggregates.Certification aggregate, string? eTag, CancellationToken cancellationToken = default);
}
