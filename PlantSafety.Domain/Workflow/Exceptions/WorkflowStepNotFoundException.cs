// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Domain
//  File:           WorkflowStepNotFoundException.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a referenced WorkflowStepId does not exist within its owning
//                  WorkflowDefinition or WorkflowInstance.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.Domain.Workflow.ValueObjects.Ids;

namespace PlantSafety.Domain.Workflow.Exceptions;

/// <summary>Thrown when a referenced WorkflowStepId does not exist within its owning aggregate.</summary>
public sealed class WorkflowStepNotFoundException : WorkflowDomainException
{
    /// <summary>Creates a step-not-found exception.</summary>
    public WorkflowStepNotFoundException(WorkflowStepId stepId, CorrelationId correlationId, CausationId causationId)
        : base(
            $"WorkflowStep {stepId} was not found.",
            "Workflow.StepNotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
