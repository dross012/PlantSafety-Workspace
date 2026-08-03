// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Domain
//  File:           WorkflowRetryLimitExceededException.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a caller explicitly requests another retry after the step's bounded
//                  retry budget (RetryPolicy.MaxAttempts) has already been exhausted.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.Domain.Workflow.ValueObjects.Ids;

namespace PlantSafety.Domain.Workflow.Exceptions;

/// <summary>Thrown when a retry is requested after the step's bounded retry budget is exhausted.</summary>
public sealed class WorkflowRetryLimitExceededException : WorkflowDomainException
{
    /// <summary>Creates a retry-limit-exceeded exception.</summary>
    public WorkflowRetryLimitExceededException(WorkflowStepId stepId, int attemptsMade, CorrelationId correlationId, CausationId causationId)
        : base(
            $"WorkflowStep {stepId} has exhausted its retry budget after {attemptsMade} attempt(s).",
            "Workflow.RetryLimitExceeded",
            "Lifecycle",
            "High",
            correlationId,
            causationId)
    {
    }
}
