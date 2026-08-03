// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Domain
//  File:           IWorkflowInstanceFactory.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Factory interface for starting WorkflowInstance aggregates from a
//                  WorkflowDefinition.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Workflow.Aggregates;

namespace PlantSafety.Domain.Workflow.Interfaces;

/// <summary>Factory interface for starting WorkflowInstance aggregates from a WorkflowDefinition.</summary>
public interface IWorkflowInstanceFactory
{
    /// <summary>Starts a new WorkflowInstance from the given, already-activated WorkflowDefinition.</summary>
    WorkflowInstance Start(
        WorkflowDefinition definition,
        UserId initiatedBy,
        UtcTimestamp nowUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        IReadOnlyDictionary<string, string>? initialContextData = null);
}
