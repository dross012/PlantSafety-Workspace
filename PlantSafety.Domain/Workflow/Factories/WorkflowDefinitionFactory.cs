// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Domain
//  File:           WorkflowDefinitionFactory.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing WorkflowDefinition aggregates.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Workflow.Aggregates;
using PlantSafety.Domain.Workflow.Enums;
using PlantSafety.Domain.Workflow.Interfaces;

namespace PlantSafety.Domain.Workflow.Factories;

/// <summary>Default factory implementation for constructing WorkflowDefinition aggregates.</summary>
public sealed class WorkflowDefinitionFactory : IWorkflowDefinitionFactory
{
    /// <inheritdoc/>
    public WorkflowDefinition Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        WorkflowType workflowType,
        string name,
        string description)
        => WorkflowDefinition.Create(createdBy, createdAtUtc, correlationId, causationId, actor, tenantId, siteId, workflowType, name, description);
}
