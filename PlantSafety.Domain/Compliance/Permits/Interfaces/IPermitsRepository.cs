// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Permits
//  Layer:          Domain
//  File:           IPermitsRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the Permits aggregate root.
//                  Additive-only evolution.
//  UPDATED:        2026-07-30 -- Phase 4 full build-out: GetByIdAsync now takes a tenantId
//                  parameter (tenant-scoped from the start, mirroring
//                  IAccidentRepository.GetByIdAsync(AccidentId, Guid, CancellationToken)) and
//                  UpdateAsync now takes an optimistic-concurrency ETag (mirroring
//                  IPermitWorkflowRepository.UpdateAsync(PermitWorkflow, string?, CancellationToken)).
//                  This aggregate previously had no write path at all -- no Application-layer
//                  command ever called these methods -- so there is no pre-existing 2-arg call
//                  site to migrate, unlike the retrofits applied to PermitToWork/RiskManagement/
//                  AccidentManagement.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Permits.Interfaces;

/// <summary>Domain repository interface for the <see cref="Permits"/> aggregate root.</summary>
public interface IPermitsRepository
{
    /// <summary>Retrieves a Permits aggregate by its identity, scoped to the given tenant, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.Compliance.Permits.Aggregates.Permits?> GetByIdAsync(PermitsId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Returns all Permits aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.Compliance.Permits.Aggregates.Permits>> ListAsync(PermitsSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new Permits aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.Compliance.Permits.Aggregates.Permits aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes to an existing Permits aggregate, enforcing optimistic concurrency.
    /// </summary>
    /// <param name="aggregate">The mutated aggregate to persist.</param>
    /// <param name="eTag">
    /// The last-known concurrency token (from a prior read). A stale or mismatched value causes
    /// the persistence layer to reject the write as a concurrency conflict (zero rows affected).
    /// </param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task UpdateAsync(PlantSafety.Domain.Compliance.Permits.Aggregates.Permits aggregate, string? eTag, CancellationToken cancellationToken = default);
}
