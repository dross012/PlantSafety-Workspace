// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Auditing
//  Layer:          Domain
//  File:           AuditingLifecycleEvents.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Domain events for the Auditing aggregate's full lifecycle
//                  (Start/RecordFindings/Close/Archive), raised alongside the pre-existing
//                  AuditingCreatedEvent. Phase 4 full build-out -- these transitions previously
//                  did not exist on the aggregate at all. Mirrors
//                  PlantSafety.Domain.Compliance.Permits.Events.PermitsLifecycleEvents.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Auditing.Events;

/// <summary>Raised when an audit program/execution is started (Created -&gt; Active).</summary>
public sealed record AuditingStartedEvent(
    AuditingEventId EventId,
    AuditingId AuditingId,
    UserId StartedBy,
    UtcTimestamp StartedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAuditingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AuditingStarted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => StartedAtUtc;
}

/// <summary>Raised when an audit's findings and compliance score are recorded (Active -&gt; Completed).</summary>
public sealed record AuditingFindingsRecordedEvent(
    AuditingEventId EventId,
    AuditingId AuditingId,
    UserId CompletedBy,
    int FindingsCount,
    decimal? Score,
    string ComplianceRating,
    UtcTimestamp CompletedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAuditingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AuditingFindingsRecorded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CompletedAtUtc;
}

/// <summary>Raised when a completed audit is formally closed (Completed -&gt; Closed).</summary>
public sealed record AuditingClosedEvent(
    AuditingEventId EventId,
    AuditingId AuditingId,
    UserId ClosedBy,
    UtcTimestamp ClosedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAuditingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AuditingClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ClosedAtUtc;
}

/// <summary>Raised when a closed audit is archived for compliance retention (Closed -&gt; Archived).</summary>
public sealed record AuditingArchivedEvent(
    AuditingEventId EventId,
    AuditingId AuditingId,
    UtcTimestamp ArchivedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAuditingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AuditingArchived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ArchivedAtUtc;
}
