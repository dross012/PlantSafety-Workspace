// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Engagement
//  Layer:          Domain
//  File:           EngagementLifecycleEvents.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Domain events for the Engagement aggregate's lifecycle (Review/
//                  RecordTouchpoint/Complete/Close/Archive), raised alongside the pre-existing
//                  EngagementCreatedEvent. Phase 4 full build-out -- these transitions previously
//                  did not exist on the aggregate at all (confirmed zero Application-layer callers
//                  under PlantSafety.Application\Analytics\Engagement). Mirrors
//                  PermitsLifecycleEvents.cs's identical precedent.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.Engagement.Events;

/// <summary>Raised when a new Engagement measurement is reviewed and activated (Created -&gt; Active).</summary>
public sealed record EngagementReviewedEvent(
    EngagementEventId EventId,
    EngagementId EngagementId,
    UserId ReviewedByUserId,
    UtcTimestamp ReviewedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEngagementDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EngagementReviewed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ReviewedAtUtc;
}

/// <summary>Raised when a worker touchpoint is recorded against an active Engagement measurement (no status change).</summary>
public sealed record EngagementTouchpointRecordedEvent(
    EngagementEventId EventId,
    EngagementId EngagementId,
    UtcTimestamp TouchedAtUtc,
    int ResponseCount,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEngagementDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EngagementTouchpointRecorded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => TouchedAtUtc;
}

/// <summary>Raised when an Engagement measurement is completed (Active -&gt; Completed).</summary>
public sealed record EngagementCompletedEvent(
    EngagementEventId EventId,
    EngagementId EngagementId,
    UserId CompletedByUserId,
    UtcTimestamp CompletedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEngagementDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EngagementCompleted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CompletedAtUtc;
}

/// <summary>Raised when a completed Engagement measurement is formally closed (Completed -&gt; Closed).</summary>
public sealed record EngagementClosedEvent(
    EngagementEventId EventId,
    EngagementId EngagementId,
    UserId ClosedByUserId,
    UtcTimestamp ClosedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEngagementDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EngagementClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ClosedAtUtc;
}

/// <summary>Raised when a closed Engagement measurement is archived for long-term retention (Closed -&gt; Archived).</summary>
public sealed record EngagementArchivedEvent(
    EngagementEventId EventId,
    EngagementId EngagementId,
    UtcTimestamp ArchivedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEngagementDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EngagementArchived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ArchivedAtUtc;
}
