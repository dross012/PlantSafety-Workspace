// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Domain
//  File:           IWorkflowDefinitionRepository.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the WorkflowDefinition aggregate root.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Workflow.Aggregates;
using PlantSafety.Domain.Workflow.ValueObjects.Ids;

namespace PlantSafety.Domain.Workflow.Interfaces;

/// <summary>Domain repository interface for the WorkflowDefinition aggregate root.</summary>
public interface IWorkflowDefinitionRepository
{
    /// <summary>
    /// Retrieves a WorkflowDefinition aggregate by its identity, scoped to the given tenant, or
    /// <see langword="null"/> if not found or if the definition belongs to a different tenant.
    /// </summary>
    Task<WorkflowDefinition?> GetByIdAsync(WorkflowDefinitionId id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Returns all active WorkflowDefinition aggregates for the given tenant.</summary>
    Task<IReadOnlyList<WorkflowDefinition>> ListActiveAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Persists a new WorkflowDefinition aggregate.</summary>
    Task AddAsync(WorkflowDefinition aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing WorkflowDefinition aggregate, enforcing optimistic concurrency.</summary>
    /// <param name="aggregate">The mutated aggregate to persist.</param>
    /// <param name="eTag">The last-known concurrency token (from a prior read). A stale value causes a concurrency conflict.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task UpdateAsync(WorkflowDefinition aggregate, string? eTag, CancellationToken cancellationToken = default);
}
