// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Culture
//  Layer:          Domain
//  File:           CultureLifecycleEvents.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Domain events for the Culture aggregate's full lifecycle
//                  (Review/Publish/Close/Archive), raised alongside the pre-existing
//                  CultureCreatedEvent. Phase 4 full build-out -- these transitions previously did
//                  not exist on the aggregate at all. Mirrors
//                  PlantSafety.Domain.Compliance.Permits.Events.PermitsLifecycleEvents.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.Culture.Events;

/// <summary>Raised when a submitted survey period is reviewed (Created -&gt; Active).</summary>
public sealed record CultureReviewedEvent(
    CultureEventId EventId,
    CultureId CultureId,
    UserId ReviewedByUserId,
    UtcTimestamp ReviewedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ICultureDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CultureReviewed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ReviewedAtUtc;
}

/// <summary>Raised when a reviewed survey period's results are published (Active -&gt; Completed).</summary>
public sealed record CulturePublishedEvent(
    CultureEventId EventId,
    CultureId CultureId,
    UserId PublishedByUserId,
    UtcTimestamp PublishedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ICultureDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CulturePublished";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => PublishedAtUtc;
}

/// <summary>Raised when a published survey period is formally closed (Completed -&gt; Closed).</summary>
public sealed record CultureClosedEvent(
    CultureEventId EventId,
    CultureId CultureId,
    UserId ClosedByUserId,
    UtcTimestamp ClosedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ICultureDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CultureClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ClosedAtUtc;
}

/// <summary>Raised when a closed survey period is archived for long-term retention (Closed -&gt; Archived).</summary>
public sealed record CultureArchivedEvent(
    CultureEventId EventId,
    CultureId CultureId,
    UtcTimestamp ArchivedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ICultureDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CultureArchived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ArchivedAtUtc;
}
