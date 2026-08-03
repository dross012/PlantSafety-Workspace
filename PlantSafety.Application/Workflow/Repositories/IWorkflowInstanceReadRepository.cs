// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Application
//  File:           IWorkflowInstanceReadRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for the WorkflowInstance read-model,
//                  including pending-approvals-for-actor and full step history queries.
//                  Implemented in the Infrastructure layer. Distinct simple name from the
//                  Domain-layer (write-side) IWorkflowInstanceRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Workflow.Models;

namespace PlantSafety.Application.Workflow.Repositories;

/// <summary>Read-only repository for WorkflowInstance read-model data. Implemented in the Infrastructure layer.</summary>
public interface IWorkflowInstanceReadRepository
{
    /// <summary>
    /// Returns the WorkflowInstance read-model record for the given identity, scoped to the given
    /// tenant, or <see langword="null"/> if not found or if the record belongs to a different
    /// tenant.
    /// </summary>
    Task<WorkflowInstanceDto?> GetByIdAsync(Guid workflowInstanceId, Guid tenantId, CancellationToken cancellationToken);

    /// <summary>
    /// Returns all WorkflowInstance records currently waiting on a HumanApproval step assigned
    /// either directly to <paramref name="userId"/> or to one of <paramref name="userRoles"/>.
    /// </summary>
    Task<IReadOnlyList<WorkflowInstanceDto>> ListPendingApprovalsForActorAsync(
        Guid tenantId, Guid userId, IReadOnlyList<string> userRoles, CancellationToken cancellationToken);

    /// <summary>Returns the full step execution history for the given WorkflowInstance, in activation order.</summary>
    Task<IReadOnlyList<WorkflowStepHistoryEntryDto>> ListHistoryAsync(
        Guid workflowInstanceId, Guid tenantId, CancellationToken cancellationToken);
}
