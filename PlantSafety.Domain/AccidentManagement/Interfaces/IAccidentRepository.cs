// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AccidentManagement
//  Layer:          Domain
//  File:           IAccidentRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the Accident aggregate root.
//                  Additive-only evolution.
//  UPDATED:        2026-07-29 -- UpdateAsync now takes an optimistic-concurrency ETag (Phase 4
//                  concurrency remediation, mirrors IRiskRepository.UpdateAsync /
//                  IPermitWorkflowRepository.UpdateAsync). Callers must supply the last-known ETag;
//                  a stale value causes ConcurrencyConflictException rather than a blind overwrite.
// ============================================================================
using PlantSafety.Domain.AccidentManagement.Aggregates;
using PlantSafety.Domain.AccidentManagement.Specifications;
using PlantSafety.Domain.AccidentManagement.ValueObjects.Ids;

namespace PlantSafety.Domain.AccidentManagement.Interfaces;

/// <summary>Domain repository interface for the Accident aggregate root.</summary>
public interface IAccidentRepository
{
    /// <summary>
    /// Retrieves a Accident aggregate by its identity, scoped to <paramref name="tenantId"/>, or
    /// <see langword="null"/> if not found (either because no such id exists, or it belongs to a
    /// different tenant). <paramref name="tenantId"/> is required so the Infrastructure
    /// implementation can populate SQL Server SESSION_CONTEXT('TenantId') for Row-Level Security
    /// and enforce the tenant boundary in the query itself -- see
    /// docs\lifecycle\security\rls-audit.md.
    /// </summary>
    Task<Accident?> GetByIdAsync(AccidentId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Returns all Accident aggregates matching the given specification.</summary>
    Task<IReadOnlyList<Accident>> ListAsync(AccidentSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new Accident aggregate.</summary>
    Task AddAsync(Accident aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes to an existing Accident aggregate, enforcing optimistic concurrency.
    /// </summary>
    /// <param name="aggregate">The mutated aggregate to persist.</param>
    /// <param name="eTag">
    /// The last-known concurrency token (from a prior read). A stale or mismatched value causes
    /// the persistence layer to reject the write as a concurrency conflict (zero rows affected).
    /// </param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task UpdateAsync(Accident aggregate, string? eTag, CancellationToken cancellationToken = default);
}