// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PredictiveMaintenance
//  Layer:          Domain
//  File:           PredictiveMaintenanceAlertEvents.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Domain events for the PredictiveMaintenanceAlert aggregate's full lifecycle
//                  (Raise/Review/ScheduleAction/Resolve/Dismiss/Close). Phase 4 full build-out --
//                  this aggregate did not exist before this pass (only a placeholder marker class
//                  and a read-only query slice existed).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.PredictiveMaintenance.Events;

/// <summary>Raised when a new predictive-failure alert is created.</summary>
public sealed record PredictiveMaintenanceAlertRaisedEvent(
    PredictiveMaintenanceEventId EventId,
    PredictiveMaintenanceAlertId PredictiveMaintenanceAlertId,
    Guid EquipmentId,
    string PredictedFailureType,
    UtcTimestamp PredictedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPredictiveMaintenanceAlertDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PredictiveMaintenanceAlertRaised";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => PredictedAtUtc;
}

/// <summary>Raised when a reviewer triages an alert (Raised -&gt; UnderReview).</summary>
public sealed record PredictiveMaintenanceAlertReviewedEvent(
    PredictiveMaintenanceEventId EventId,
    PredictiveMaintenanceAlertId PredictiveMaintenanceAlertId,
    UserId ReviewedBy,
    UtcTimestamp ReviewedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPredictiveMaintenanceAlertDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PredictiveMaintenanceAlertReviewed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ReviewedAtUtc;
}

/// <summary>Raised when corrective maintenance action is scheduled against an alert (UnderReview -&gt; ActionScheduled).</summary>
public sealed record PredictiveMaintenanceAlertActionScheduledEvent(
    PredictiveMaintenanceEventId EventId,
    PredictiveMaintenanceAlertId PredictiveMaintenanceAlertId,
    UserId ScheduledBy,
    string? WorkOrderReference,
    UtcTimestamp ScheduledAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPredictiveMaintenanceAlertDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PredictiveMaintenanceAlertActionScheduled";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ScheduledAtUtc;
}

/// <summary>Raised when the predicted failure has been addressed (ActionScheduled -&gt; Resolved).</summary>
public sealed record PredictiveMaintenanceAlertResolvedEvent(
    PredictiveMaintenanceEventId EventId,
    PredictiveMaintenanceAlertId PredictiveMaintenanceAlertId,
    UserId ResolvedBy,
    string? ResolutionNotes,
    UtcTimestamp ResolvedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPredictiveMaintenanceAlertDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PredictiveMaintenanceAlertResolved";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ResolvedAtUtc;
}

/// <summary>Raised when an alert is dismissed without corrective action (Raised/UnderReview -&gt; Dismissed).</summary>
public sealed record PredictiveMaintenanceAlertDismissedEvent(
    PredictiveMaintenanceEventId EventId,
    PredictiveMaintenanceAlertId PredictiveMaintenanceAlertId,
    UserId DismissedBy,
    string Reason,
    UtcTimestamp DismissedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPredictiveMaintenanceAlertDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PredictiveMaintenanceAlertDismissed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => DismissedAtUtc;
}

/// <summary>Raised when a resolved or dismissed alert is formally closed (Resolved/Dismissed -&gt; Closed).</summary>
public sealed record PredictiveMaintenanceAlertClosedEvent(
    PredictiveMaintenanceEventId EventId,
    PredictiveMaintenanceAlertId PredictiveMaintenanceAlertId,
    UserId ClosedBy,
    UtcTimestamp ClosedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPredictiveMaintenanceAlertDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PredictiveMaintenanceAlertClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ClosedAtUtc;
}
