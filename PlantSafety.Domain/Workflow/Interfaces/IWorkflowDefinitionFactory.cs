// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Domain
//  File:           IWorkflowDefinitionFactory.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing WorkflowDefinition aggregates.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Workflow.Aggregates;
using PlantSafety.Domain.Workflow.Enums;

namespace PlantSafety.Domain.Workflow.Interfaces;

/// <summary>Factory interface for constructing WorkflowDefinition aggregates.</summary>
public interface IWorkflowDefinitionFactory
{
    /// <summary>Creates a new, inactive WorkflowDefinition with the supplied parameters.</summary>
    WorkflowDefinition Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        WorkflowType workflowType,
        string name,
        string description);
}
