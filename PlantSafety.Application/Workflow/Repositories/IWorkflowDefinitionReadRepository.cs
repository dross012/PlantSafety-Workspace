// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Application
//  File:           IWorkflowDefinitionReadRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for the WorkflowDefinition read-model.
//                  Implemented in the Infrastructure layer. Distinct simple name from the
//                  Domain-layer (write-side) IWorkflowDefinitionRepository -- same disambiguation
//                  pattern as
//                  PlantSafety.Application.Operational.PreventiveMaintenance.Repositories.IPreventiveMaintenanceTaskRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Workflow.Models;

namespace PlantSafety.Application.Workflow.Repositories;

/// <summary>Read-only repository for WorkflowDefinition read-model data. Implemented in the Infrastructure layer.</summary>
public interface IWorkflowDefinitionReadRepository
{
    /// <summary>
    /// Returns the WorkflowDefinition read-model record (including its full step graph) for the
    /// given identity, scoped to the given tenant, or <see langword="null"/> if not found or if
    /// the record belongs to a different tenant.
    /// </summary>
    Task<WorkflowDefinitionDto?> GetByIdAsync(Guid workflowDefinitionId, Guid tenantId, CancellationToken cancellationToken);
}
