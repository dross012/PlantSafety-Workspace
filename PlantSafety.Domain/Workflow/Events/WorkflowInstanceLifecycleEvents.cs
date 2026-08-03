// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Domain
//  File:           WorkflowInstanceLifecycleEvents.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain events raised by the WorkflowInstance aggregate for overall lifecycle
//                  transitions (start, completion, rejection, cancellation, failure). Every event
//                  carries the canonical causality/correlation envelope (Handbook §2.17-2.18).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Workflow.ValueObjects.Ids;

namespace PlantSafety.Domain.Workflow.Events;

/// <summary>Raised when a new WorkflowInstance is started from a WorkflowDefinition.</summary>
public sealed record WorkflowStartedEvent(
    WorkflowEventId EventId,
    WorkflowInstanceId WorkflowInstanceId,
    WorkflowDefinitionId WorkflowDefinitionId,
    UtcTimestamp StartedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IWorkflowDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "WorkflowStarted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => StartedAtUtc;
}

/// <summary>Raised whenever a WorkflowInstance's current step advances to a new step (including the initial activation).</summary>
public sealed record WorkflowStepAdvancedEvent(
    WorkflowEventId EventId,
    WorkflowInstanceId WorkflowInstanceId,
    WorkflowStepId? FromStepId,
    WorkflowStepId ToStepId,
    UtcTimestamp AdvancedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IWorkflowDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "WorkflowStepAdvanced";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => AdvancedAtUtc;
}

/// <summary>Raised when the WorkflowInstance completes all steps successfully.</summary>
public sealed record WorkflowCompletedEvent(
    WorkflowEventId EventId,
    WorkflowInstanceId WorkflowInstanceId,
    UtcTimestamp CompletedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IWorkflowDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "WorkflowCompleted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CompletedAtUtc;
}

/// <summary>Raised when the WorkflowInstance is rejected at a human-approval step.</summary>
public sealed record WorkflowRejectedEvent(
    WorkflowEventId EventId,
    WorkflowInstanceId WorkflowInstanceId,
    WorkflowStepId StepId,
    UserId RejectedBy,
    string? Reason,
    UtcTimestamp RejectedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IWorkflowDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "WorkflowRejected";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => RejectedAtUtc;
}

/// <summary>Raised when the WorkflowInstance is cancelled before completion.</summary>
public sealed record WorkflowCancelledEvent(
    WorkflowEventId EventId,
    WorkflowInstanceId WorkflowInstanceId,
    string? Reason,
    UtcTimestamp CancelledAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IWorkflowDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "WorkflowCancelled";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CancelledAtUtc;
}

/// <summary>Raised when the WorkflowInstance exhausts its bounded retry budget for the current step and cannot proceed.</summary>
public sealed record WorkflowFailedEvent(
    WorkflowEventId EventId,
    WorkflowInstanceId WorkflowInstanceId,
    WorkflowStepId StepId,
    string Reason,
    UtcTimestamp FailedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IWorkflowDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "WorkflowFailed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => FailedAtUtc;
}
