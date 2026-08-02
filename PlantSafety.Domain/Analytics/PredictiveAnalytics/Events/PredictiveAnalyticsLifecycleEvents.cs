// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.PredictiveAnalytics
//  Layer:          Domain
//  File:           PredictiveAnalyticsLifecycleEvents.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Domain events for the PredictiveAnalytics aggregate's lifecycle (Review/
//                  Action/Close/Archive -- grounded in the documented status vocabulary
//                  "Generated/Reviewed/Actioned" on PredictiveAnalyticsItemDto.Status), raised
//                  alongside the pre-existing PredictiveAnalyticsCreatedEvent. Phase 4 full
//                  build-out -- these transitions previously did not exist on the aggregate at
//                  all. Mirrors PermitsLifecycleEvents.cs's identical precedent.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.PredictiveAnalytics.Events;

/// <summary>Raised when a generated prediction is reviewed (Created -&gt; Active, i.e. "Reviewed").</summary>
public sealed record PredictiveAnalyticsReviewedEvent(
    PredictiveAnalyticsEventId EventId,
    PredictiveAnalyticsId PredictiveAnalyticsId,
    UserId ReviewedByUserId,
    UtcTimestamp ReviewedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPredictiveAnalyticsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PredictiveAnalyticsReviewed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ReviewedAtUtc;
}

/// <summary>Raised when a reviewed prediction is acted upon (Active -&gt; Completed, i.e. "Actioned").</summary>
public sealed record PredictiveAnalyticsActionedEvent(
    PredictiveAnalyticsEventId EventId,
    PredictiveAnalyticsId PredictiveAnalyticsId,
    UserId ActionedByUserId,
    string? ActionNotes,
    UtcTimestamp ActionedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPredictiveAnalyticsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PredictiveAnalyticsActioned";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ActionedAtUtc;
}

/// <summary>Raised when an actioned prediction is formally closed (Completed -&gt; Closed).</summary>
public sealed record PredictiveAnalyticsClosedEvent(
    PredictiveAnalyticsEventId EventId,
    PredictiveAnalyticsId PredictiveAnalyticsId,
    UserId ClosedByUserId,
    UtcTimestamp ClosedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPredictiveAnalyticsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PredictiveAnalyticsClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ClosedAtUtc;
}

/// <summary>Raised when a closed prediction is archived for long-term retention (Closed -&gt; Archived).</summary>
public sealed record PredictiveAnalyticsArchivedEvent(
    PredictiveAnalyticsEventId EventId,
    PredictiveAnalyticsId PredictiveAnalyticsId,
    UtcTimestamp ArchivedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPredictiveAnalyticsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PredictiveAnalyticsArchived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ArchivedAtUtc;
}
