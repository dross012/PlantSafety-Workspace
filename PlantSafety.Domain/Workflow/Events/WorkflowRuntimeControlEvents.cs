// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Domain
//  File:           WorkflowRuntimeControlEvents.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain events raised by the WorkflowInstance runtime engine's control-flow
//                  mechanisms: timeout, escalation, and bounded retry-with-backoff (Phase 5.2
//                  workflow runtime). Every event carries the canonical causality/correlation
//                  envelope (Handbook §2.17-2.18).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Workflow.ValueObjects.Ids;

namespace PlantSafety.Domain.Workflow.Events;

/// <summary>Raised when a workflow step exceeds its configured timeout window.</summary>
public sealed record WorkflowTimedOutEvent(
    WorkflowEventId EventId,
    WorkflowInstanceId WorkflowInstanceId,
    WorkflowStepId StepId,
    UtcTimestamp TimedOutAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IWorkflowDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "WorkflowTimedOut";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => TimedOutAtUtc;
}

/// <summary>Raised when a workflow step is escalated to a designated escalation actor after timeout.</summary>
public sealed record WorkflowEscalatedEvent(
    WorkflowEventId EventId,
    WorkflowInstanceId WorkflowInstanceId,
    WorkflowStepId StepId,
    string EscalatedTo,
    UtcTimestamp EscalatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IWorkflowDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "WorkflowEscalated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => EscalatedAtUtc;
}

/// <summary>Raised when a bounded retry attempt is scheduled for a failed workflow step.</summary>
public sealed record WorkflowRetryScheduledEvent(
    WorkflowEventId EventId,
    WorkflowInstanceId WorkflowInstanceId,
    WorkflowStepId StepId,
    int AttemptNumber,
    UtcTimestamp NextAttemptAtUtc,
    UtcTimestamp ScheduledAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IWorkflowDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "WorkflowRetryScheduled";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ScheduledAtUtc;
}

/// <summary>Raised when a workflow step's bounded retry budget is exhausted.</summary>
public sealed record WorkflowRetryLimitExceededEvent(
    WorkflowEventId EventId,
    WorkflowInstanceId WorkflowInstanceId,
    WorkflowStepId StepId,
    int AttemptsMade,
    UtcTimestamp OccurredAt,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IWorkflowDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "WorkflowRetryLimitExceeded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => OccurredAt;
}
