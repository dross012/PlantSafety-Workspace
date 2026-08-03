// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Domain
//  File:           WorkflowInstanceFactory.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for starting WorkflowInstance aggregates from an
//                  already-activated WorkflowDefinition.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Workflow.Aggregates;
using PlantSafety.Domain.Workflow.Interfaces;

namespace PlantSafety.Domain.Workflow.Factories;

/// <summary>Default factory implementation for starting WorkflowInstance aggregates.</summary>
public sealed class WorkflowInstanceFactory : IWorkflowInstanceFactory
{
    /// <inheritdoc/>
    public WorkflowInstance Start(
        WorkflowDefinition definition,
        UserId initiatedBy,
        UtcTimestamp nowUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        IReadOnlyDictionary<string, string>? initialContextData = null)
    {
        if (!definition.IsActive)
            throw new InvalidOperationException($"WorkflowDefinition {definition.WorkflowDefinitionId} is not active; cannot start a new instance.");

        var firstStep = definition.GetStartStep();

        return WorkflowInstance.Start(
            definition.WorkflowDefinitionId,
            definition.WorkflowType,
            firstStep.StepId,
            firstStep.StepType,
            firstStep.AssignedActor,
            initiatedBy,
            nowUtc,
            correlationId,
            causationId,
            actor,
            tenantId,
            siteId,
            initialContextData);
    }
}
