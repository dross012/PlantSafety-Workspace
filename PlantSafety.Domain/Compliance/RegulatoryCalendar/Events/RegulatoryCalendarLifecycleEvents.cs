// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RegulatoryCalendar
//  Layer:          Domain
//  File:           RegulatoryCalendarLifecycleEvents.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Domain events for the RegulatoryCalendar aggregate's full lifecycle
//                  (Activate/RecordCompliance/Close/Archive), raised alongside the pre-existing
//                  RegulatoryCalendarCreatedEvent. Phase 4 full build-out -- these transitions
//                  previously did not exist on the aggregate at all. Mirrors
//                  PermitsLifecycleEvents.cs.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.RegulatoryCalendar.Events;

/// <summary>Raised when a RegulatoryCalendar obligation is activated (Created -&gt; Active).</summary>
public sealed record RegulatoryCalendarActivatedEvent(
    RegulatoryCalendarEventId EventId,
    RegulatoryCalendarId RegulatoryCalendarId,
    UserId ActivatedByUserId,
    UtcTimestamp ActivatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IRegulatoryCalendarDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "RegulatoryCalendarActivated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ActivatedAtUtc;
}

/// <summary>Raised when compliance is recorded for an active obligation (Active -&gt; Completed).</summary>
public sealed record RegulatoryCalendarComplianceRecordedEvent(
    RegulatoryCalendarEventId EventId,
    RegulatoryCalendarId RegulatoryCalendarId,
    UserId CompletedByUserId,
    UtcTimestamp CompletedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IRegulatoryCalendarDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "RegulatoryCalendarComplianceRecorded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CompletedAtUtc;
}

/// <summary>Raised when a completed obligation is formally closed (Completed -&gt; Closed).</summary>
public sealed record RegulatoryCalendarClosedEvent(
    RegulatoryCalendarEventId EventId,
    RegulatoryCalendarId RegulatoryCalendarId,
    UserId ClosedByUserId,
    UtcTimestamp ClosedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IRegulatoryCalendarDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "RegulatoryCalendarClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ClosedAtUtc;
}

/// <summary>Raised when a closed obligation is archived for compliance retention (Closed -&gt; Archived).</summary>
public sealed record RegulatoryCalendarArchivedEvent(
    RegulatoryCalendarEventId EventId,
    RegulatoryCalendarId RegulatoryCalendarId,
    UtcTimestamp ArchivedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IRegulatoryCalendarDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "RegulatoryCalendarArchived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ArchivedAtUtc;
}
