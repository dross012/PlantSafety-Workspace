// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitWorkflow
//  Layer:          Domain
//  File:           IPermitWorkflowRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the PermitWorkflow aggregate root.
//                  Additive-only evolution.
//  UPDATED:        2026-07-29 -- UpdateAsync now takes an optimistic-concurrency ETag (Gap 2
//                  remediation). The ConcurrencyToken UNIQUEIDENTIFIER column was regenerated on
//                  every UPDATE but never checked; the backing table now also carries a native
//                  RowVersion column, and callers must supply the last-known ETag (obtained from
//                  a prior read) so a stale write fails fast with ConcurrencyConflictException
//                  rather than silently overwriting concurrent changes. Mirrors
//                  IIncidentRepository.UpdateAsync's eTag parameter.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitWorkflow.Interfaces;

/// <summary>Domain repository interface for the <see cref="PermitWorkflow"/> aggregate root.</summary>
public interface IPermitWorkflowRepository
{
    /// <summary>Retrieves a PermitWorkflow aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.PermitToWork.PermitWorkflow.Aggregates.PermitWorkflow?> GetByIdAsync(PermitWorkflowId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all PermitWorkflow aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.PermitToWork.PermitWorkflow.Aggregates.PermitWorkflow>> ListAsync(PermitWorkflowSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new PermitWorkflow aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.PermitToWork.PermitWorkflow.Aggregates.PermitWorkflow aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes to an existing PermitWorkflow aggregate, enforcing optimistic concurrency.
    /// </summary>
    /// <param name="aggregate">The mutated aggregate to persist.</param>
    /// <param name="eTag">
    /// The last-known concurrency token (from a prior read). A stale or mismatched value causes
    /// the persistence layer to reject the write as a concurrency conflict (zero rows affected).
    /// </param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task UpdateAsync(PlantSafety.Domain.PermitToWork.PermitWorkflow.Aggregates.PermitWorkflow aggregate, string? eTag, CancellationToken cancellationToken = default);
}