// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Domain
//  File:           WorkflowDefinitionNotFoundException.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a referenced WorkflowDefinitionId cannot be resolved.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.Domain.Workflow.ValueObjects.Ids;

namespace PlantSafety.Domain.Workflow.Exceptions;

/// <summary>Thrown when a referenced WorkflowDefinitionId cannot be resolved.</summary>
public sealed class WorkflowDefinitionNotFoundException : WorkflowDomainException
{
    /// <summary>Creates a definition-not-found exception.</summary>
    public WorkflowDefinitionNotFoundException(WorkflowDefinitionId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"WorkflowDefinition {id} was not found.",
            "Workflow.DefinitionNotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
