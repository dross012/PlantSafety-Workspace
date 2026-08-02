// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.NearMisses
//  Layer:          Domain
//  File:           NearMissesEvents.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Domain events for the NearMisses aggregate's full lifecycle
//                  (Report/StartReview/Complete/Close/Archive). Phase 4 full build-out -- this
//                  aggregate did not exist before this pass (only a placeholder marker class and a
//                  read-only query slice existed).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.NearMisses.Events;

/// <summary>Raised when a new near-miss is reported.</summary>
public sealed record NearMissesReportedEvent(
    NearMissesEventId EventId,
    NearMissesId NearMissesId,
    Guid TenantId,
    Guid? SiteId,
    string Category,
    string PotentialSeverity,
    UtcTimestamp ReportedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : INearMissesDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "NearMissesReported";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ReportedAtUtc;
}

/// <summary>Raised when a reviewer starts triaging a near-miss (Reported -&gt; UnderReview).</summary>
public sealed record NearMissesReviewStartedEvent(
    NearMissesEventId EventId,
    NearMissesId NearMissesId,
    UserId ReviewStartedBy,
    UtcTimestamp ReviewStartedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : INearMissesDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "NearMissesReviewStarted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ReviewStartedAtUtc;
}

/// <summary>Raised when a near-miss review is completed and findings are recorded (UnderReview -&gt; Completed).</summary>
public sealed record NearMissesCompletedEvent(
    NearMissesEventId EventId,
    NearMissesId NearMissesId,
    UserId CompletedBy,
    string? Findings,
    UtcTimestamp CompletedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : INearMissesDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "NearMissesCompleted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CompletedAtUtc;
}

/// <summary>Raised when a near-miss is formally closed (Completed -&gt; Closed).</summary>
public sealed record NearMissesClosedEvent(
    NearMissesEventId EventId,
    NearMissesId NearMissesId,
    UserId ClosedBy,
    UtcTimestamp ClosedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : INearMissesDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "NearMissesClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ClosedAtUtc;
}

/// <summary>Raised when a closed near-miss is archived for compliance retention (Closed -&gt; Archived).</summary>
public sealed record NearMissesArchivedEvent(
    NearMissesEventId EventId,
    NearMissesId NearMissesId,
    UtcTimestamp ArchivedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : INearMissesDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "NearMissesArchived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ArchivedAtUtc;
}
