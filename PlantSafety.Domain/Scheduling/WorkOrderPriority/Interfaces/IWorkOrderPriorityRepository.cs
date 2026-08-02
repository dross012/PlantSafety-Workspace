// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.WorkOrderPriority
//  Layer:          Domain
//  File:           IWorkOrderPriorityRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the WorkOrderPriority aggregate root.
//                  Additive-only evolution.
//  UPDATED:        2026-07-30 -- GetByIdAsync now requires an explicit tenantId (Phase 4
//                  cross-tenant access remediation, mirrors IRiskRepository.GetByIdAsync /
//                  IIncidentRepository.GetByIdAsync): usp_WorkOrderPriority_GetById previously
//                  filtered only by WorkOrderPriorityId, so any caller who knew or guessed another
//                  tenant's WorkOrderPriorityId could read (and, via UpdateAsync on the loaded
//                  aggregate, mutate) that tenant's record. The tenant check now happens at the DB
//                  boundary.
//  UPDATED:        2026-07-30 -- UpdateAsync now takes an optimistic-concurrency ETag (Phase 4
//                  concurrency remediation, mirrors IRiskRepository.UpdateAsync). Callers must
//                  supply the last-known ETag; a stale value causes ConcurrencyConflictException
//                  rather than a blind overwrite.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.WorkOrderPriority.Interfaces;

/// <summary>Domain repository interface for the <see cref="WorkOrderPriority"/> aggregate root.</summary>
public interface IWorkOrderPriorityRepository
{
    /// <summary>
    /// Retrieves a WorkOrderPriority aggregate by its identity, scoped to the given tenant, or
    /// <see langword="null"/> if not found or if the record belongs to a different tenant.
    /// </summary>
    Task<PlantSafety.Domain.Scheduling.WorkOrderPriority.Aggregates.WorkOrderPriority?> GetByIdAsync(WorkOrderPriorityId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Returns all WorkOrderPriority aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.Scheduling.WorkOrderPriority.Aggregates.WorkOrderPriority>> ListAsync(WorkOrderPrioritySpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new WorkOrderPriority aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.Scheduling.WorkOrderPriority.Aggregates.WorkOrderPriority aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes to an existing WorkOrderPriority aggregate, enforcing optimistic
    /// concurrency.
    /// </summary>
    /// <param name="aggregate">The mutated aggregate to persist.</param>
    /// <param name="eTag">
    /// The last-known concurrency token (from a prior read). A stale or mismatched value causes
    /// the persistence layer to reject the write as a concurrency conflict (zero rows affected).
    /// </param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task UpdateAsync(PlantSafety.Domain.Scheduling.WorkOrderPriority.Aggregates.WorkOrderPriority aggregate, string? eTag, CancellationToken cancellationToken = default);
}