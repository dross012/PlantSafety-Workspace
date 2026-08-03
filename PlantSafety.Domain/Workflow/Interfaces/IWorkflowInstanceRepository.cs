// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Domain
//  File:           IWorkflowInstanceRepository.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the WorkflowInstance aggregate root.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Workflow.Aggregates;
using PlantSafety.Domain.Workflow.Specifications;
using PlantSafety.Domain.Workflow.ValueObjects.Ids;

namespace PlantSafety.Domain.Workflow.Interfaces;

/// <summary>Domain repository interface for the WorkflowInstance aggregate root.</summary>
public interface IWorkflowInstanceRepository
{
    /// <summary>
    /// Retrieves a WorkflowInstance aggregate by its identity, scoped to the given tenant, or
    /// <see langword="null"/> if not found or if the instance belongs to a different tenant.
    /// </summary>
    Task<WorkflowInstance?> GetByIdAsync(WorkflowInstanceId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Returns all WorkflowInstance aggregates for the given tenant matching the specification.</summary>
    Task<IReadOnlyList<WorkflowInstance>> ListAsync(Guid tenantId, WorkflowInstanceSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new WorkflowInstance aggregate.</summary>
    Task AddAsync(WorkflowInstance aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing WorkflowInstance aggregate, enforcing optimistic concurrency.</summary>
    /// <param name="aggregate">The mutated aggregate to persist.</param>
    /// <param name="eTag">The last-known concurrency token (from a prior read). A stale value causes a concurrency conflict.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task UpdateAsync(WorkflowInstance aggregate, string? eTag, CancellationToken cancellationToken = default);
}
