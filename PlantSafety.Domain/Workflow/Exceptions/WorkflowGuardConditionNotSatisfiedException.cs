// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Domain
//  File:           WorkflowGuardConditionNotSatisfiedException.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when no outgoing transition (guarded or default) out of the current step
//                  is satisfied by the instance's context data, and the caller required a
//                  transition to exist (used for explicit-only navigation paths; the runtime
//                  service itself falls back to Complete() rather than throwing -- see
//                  WorkflowRuntimeService remarks).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.Domain.Workflow.ValueObjects.Ids;

namespace PlantSafety.Domain.Workflow.Exceptions;

/// <summary>Thrown when a required guard condition is not satisfied by the instance's context data.</summary>
public sealed class WorkflowGuardConditionNotSatisfiedException : WorkflowDomainException
{
    /// <summary>Creates a guard-condition-not-satisfied exception.</summary>
    public WorkflowGuardConditionNotSatisfiedException(WorkflowStepId fromStepId, CorrelationId correlationId, CausationId causationId)
        : base(
            $"No guard condition out of WorkflowStep {fromStepId} was satisfied.",
            "Workflow.GuardConditionNotSatisfied",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
