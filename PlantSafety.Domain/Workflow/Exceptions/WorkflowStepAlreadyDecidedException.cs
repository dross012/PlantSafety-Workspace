// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Domain
//  File:           WorkflowStepAlreadyDecidedException.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a decision (approve/reject/automated outcome) is submitted for a
//                  step that is not the instance's currently active step, or has already been
//                  decided.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.Domain.Workflow.ValueObjects.Ids;

namespace PlantSafety.Domain.Workflow.Exceptions;

/// <summary>Thrown when a decision is submitted for a step that is not active or has already been decided.</summary>
public sealed class WorkflowStepAlreadyDecidedException : WorkflowDomainException
{
    /// <summary>Creates a step-already-decided exception.</summary>
    public WorkflowStepAlreadyDecidedException(WorkflowStepInstanceId stepInstanceId, CorrelationId correlationId, CausationId causationId)
        : base(
            $"WorkflowStepInstance {stepInstanceId} is not active and cannot accept a new decision.",
            "Workflow.StepAlreadyDecided",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
