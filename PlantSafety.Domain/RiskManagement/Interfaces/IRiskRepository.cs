// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         RiskManagement
//  Layer:          Domain
//  File:           IRiskRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the Risk aggregate root.
//                  Additive-only evolution.
//  UPDATED:        2026-07-29 -- UpdateAsync now takes an optimistic-concurrency ETag (Phase 4
//                  concurrency remediation, mirrors IPermitWorkflowRepository.UpdateAsync /
//                  IIncidentRepository.UpdateAsync). Callers must supply the last-known ETag; a
//                  stale value causes ConcurrencyConflictException rather than a blind overwrite.
//  UPDATED:        2026-07-29 -- GetByIdAsync/ListAsync now require an explicit tenantId (Phase 4
//                  cross-tenant access remediation). usp_Risk_GetById/GetAllActive previously
//                  filtered only by RiskId/IsDeleted -- any caller who knew or guessed another
//                  tenant's RiskId could read (and, via UpdateAsync on the loaded aggregate,
//                  mutate) that tenant's Risk. The tenant check now happens at the DB boundary
//                  (WHERE TenantId = @TenantId) rather than being left to callers to remember.
//                  UpdateAsync needs no interface change: the aggregate's own TenantId (set at
//                  Create/hydration, and only ever hydrated via a tenant-checked GetByIdAsync) is
//                  now also bound into usp_Risk_Update's WHERE clause by the Infrastructure
//                  implementation, mirroring IIncidentRepository.UpdateAsync's identical approach
//                  of deriving TenantId from the aggregate rather than a separate parameter.
// ============================================================================
using PlantSafety.Domain.RiskManagement.Aggregates;
using PlantSafety.Domain.RiskManagement.Specifications;
using PlantSafety.Domain.RiskManagement.ValueObjects.Ids;

namespace PlantSafety.Domain.RiskManagement.Interfaces;

/// <summary>Domain repository interface for the Risk aggregate root.</summary>
public interface IRiskRepository
{
    /// <summary>
    /// Retrieves a Risk aggregate by its identity, scoped to the given tenant, or
    /// <see langword="null"/> if not found or if the Risk belongs to a different tenant.
    /// </summary>
    Task<Risk?> GetByIdAsync(RiskId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Returns all Risk aggregates for the given tenant matching the specification.</summary>
    Task<IReadOnlyList<Risk>> ListAsync(Guid tenantId, RiskSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new Risk aggregate.</summary>
    Task AddAsync(Risk aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes to an existing Risk aggregate, enforcing optimistic concurrency.
    /// </summary>
    /// <param name="aggregate">The mutated aggregate to persist.</param>
    /// <param name="eTag">
    /// The last-known concurrency token (from a prior read). A stale or mismatched value causes
    /// the persistence layer to reject the write as a concurrency conflict (zero rows affected).
    /// </param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task UpdateAsync(Risk aggregate, string? eTag, CancellationToken cancellationToken = default);
}