// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PreventiveMaintenance
//  Layer:          Domain
//  File:           PreventiveMaintenanceTaskEvents.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Domain events for the PreventiveMaintenanceTask aggregate's full lifecycle
//                  (Schedule/Start/Complete/Verify/Cancel/MarkOverdue). Phase 4 full build-out --
//                  this aggregate did not exist before this pass (only a placeholder marker class
//                  and a read-only query slice existed).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.PreventiveMaintenance.Events;

/// <summary>Raised when a new preventive-maintenance task is scheduled.</summary>
public sealed record PreventiveMaintenanceTaskScheduledEvent(
    PreventiveMaintenanceEventId EventId,
    PreventiveMaintenanceTaskId PreventiveMaintenanceTaskId,
    Guid EquipmentId,
    string TaskDescription,
    UtcTimestamp ScheduledAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPreventiveMaintenanceTaskDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PreventiveMaintenanceTaskScheduled";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ScheduledAtUtc;
}

/// <summary>Raised when maintenance work begins on the task (Scheduled -&gt; InProgress).</summary>
public sealed record PreventiveMaintenanceTaskStartedEvent(
    PreventiveMaintenanceEventId EventId,
    PreventiveMaintenanceTaskId PreventiveMaintenanceTaskId,
    UserId StartedBy,
    UtcTimestamp StartedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPreventiveMaintenanceTaskDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PreventiveMaintenanceTaskStarted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => StartedAtUtc;
}

/// <summary>Raised when maintenance work is completed (InProgress -&gt; Completed).</summary>
public sealed record PreventiveMaintenanceTaskCompletedEvent(
    PreventiveMaintenanceEventId EventId,
    PreventiveMaintenanceTaskId PreventiveMaintenanceTaskId,
    UserId CompletedBy,
    string? CompletionNotes,
    UtcTimestamp CompletedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPreventiveMaintenanceTaskDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PreventiveMaintenanceTaskCompleted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CompletedAtUtc;
}

/// <summary>Raised when completed work is independently verified (Completed -&gt; Verified).</summary>
public sealed record PreventiveMaintenanceTaskVerifiedEvent(
    PreventiveMaintenanceEventId EventId,
    PreventiveMaintenanceTaskId PreventiveMaintenanceTaskId,
    UserId VerifiedBy,
    UtcTimestamp VerifiedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPreventiveMaintenanceTaskDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PreventiveMaintenanceTaskVerified";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => VerifiedAtUtc;
}

/// <summary>Raised when the task is cancelled before completion (Scheduled/InProgress -&gt; Cancelled).</summary>
public sealed record PreventiveMaintenanceTaskCancelledEvent(
    PreventiveMaintenanceEventId EventId,
    PreventiveMaintenanceTaskId PreventiveMaintenanceTaskId,
    UserId CancelledBy,
    string Reason,
    UtcTimestamp CancelledAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPreventiveMaintenanceTaskDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PreventiveMaintenanceTaskCancelled";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CancelledAtUtc;
}

/// <summary>Raised when a scheduled task passes its due date without starting/completing (Scheduled -&gt; Overdue).</summary>
public sealed record PreventiveMaintenanceTaskOverdueEvent(
    PreventiveMaintenanceEventId EventId,
    PreventiveMaintenanceTaskId PreventiveMaintenanceTaskId,
    UtcTimestamp OverdueAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPreventiveMaintenanceTaskDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PreventiveMaintenanceTaskOverdue";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => OverdueAtUtc;
}
