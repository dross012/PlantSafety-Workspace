// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitExpiration
//  Layer:          Domain
//  File:           IPermitExpirationRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the PermitExpiration aggregate root.
//                  Additive-only evolution.
//  UPDATED:        2026-07-29 -- UpdateAsync now takes an optimistic-concurrency ETag (Gap 2
//                  remediation). See IPermitWorkflowRepository's matching remark for full
//                  rationale. Mirrors IIncidentRepository.UpdateAsync's eTag parameter.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitExpiration.Interfaces;

/// <summary>Domain repository interface for the <see cref="PermitExpiration"/> aggregate root.</summary>
public interface IPermitExpirationRepository
{
    /// <summary>Retrieves a PermitExpiration aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.PermitToWork.PermitExpiration.Aggregates.PermitExpiration?> GetByIdAsync(PermitExpirationId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all PermitExpiration aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.PermitToWork.PermitExpiration.Aggregates.PermitExpiration>> ListAsync(PermitExpirationSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new PermitExpiration aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.PermitToWork.PermitExpiration.Aggregates.PermitExpiration aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes to an existing PermitExpiration aggregate, enforcing optimistic concurrency.
    /// </summary>
    /// <param name="aggregate">The mutated aggregate to persist.</param>
    /// <param name="eTag">
    /// The last-known concurrency token (from a prior read). A stale or mismatched value causes
    /// the persistence layer to reject the write as a concurrency conflict (zero rows affected).
    /// </param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task UpdateAsync(PlantSafety.Domain.PermitToWork.PermitExpiration.Aggregates.PermitExpiration aggregate, string? eTag, CancellationToken cancellationToken = default);
}