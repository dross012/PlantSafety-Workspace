// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.ScoreCards
//  Layer:          Domain
//  File:           ScoreCardsLifecycleEvents.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Domain events for the ScoreCards aggregate's full lifecycle
//                  (Publish/Finalize/Close/Archive), raised alongside the pre-existing
//                  ScoreCardsCreatedEvent. Phase 4 full build-out -- these transitions
//                  previously did not exist on the aggregate at all.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.ScoreCards.Events;

/// <summary>Raised when a ScoreCards record is published (Created -&gt; Active).</summary>
public sealed record ScoreCardsPublishedEvent(
    ScoreCardsEventId EventId,
    ScoreCardsId ScoreCardsId,
    UserId PublishedByUserId,
    UtcTimestamp PublishedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IScoreCardsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ScoreCardsPublished";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => PublishedAtUtc;
}

/// <summary>Raised when a published scorecard is finalized (Active -&gt; Completed).</summary>
public sealed record ScoreCardsFinalizedEvent(
    ScoreCardsEventId EventId,
    ScoreCardsId ScoreCardsId,
    UserId FinalizedByUserId,
    UtcTimestamp FinalizedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IScoreCardsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ScoreCardsFinalized";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => FinalizedAtUtc;
}

/// <summary>Raised when a finalized scorecard is formally closed (Completed -&gt; Closed).</summary>
public sealed record ScoreCardsClosedEvent(
    ScoreCardsEventId EventId,
    ScoreCardsId ScoreCardsId,
    UserId ClosedByUserId,
    UtcTimestamp ClosedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IScoreCardsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ScoreCardsClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ClosedAtUtc;
}

/// <summary>Raised when a closed scorecard is archived for long-term retention (Closed -&gt; Archived).</summary>
public sealed record ScoreCardsArchivedEvent(
    ScoreCardsEventId EventId,
    ScoreCardsId ScoreCardsId,
    UtcTimestamp ArchivedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IScoreCardsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ScoreCardsArchived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ArchivedAtUtc;
}
