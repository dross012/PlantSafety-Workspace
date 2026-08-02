// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Dashboard
//  Layer:          Domain
//  File:           DashboardLifecycleEvents.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Domain events for the Dashboard aggregate's full lifecycle
//                  (Publish/RefreshData/Deprecate/Close/Archive), raised alongside the
//                  pre-existing DashboardCreatedEvent. Phase 4 full build-out -- these transitions
//                  previously did not exist on the aggregate at all. Mirrors
//                  PlantSafety.Domain.Compliance.Permits.Events.PermitsLifecycleEvents.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.Dashboard.Events;

/// <summary>Raised when a dashboard is published for general use (Created -&gt; Active).</summary>
public sealed record DashboardPublishedEvent(
    DashboardEventId EventId,
    DashboardId DashboardId,
    UserId PublishedByUserId,
    UtcTimestamp PublishedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IDashboardDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "DashboardPublished";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => PublishedAtUtc;
}

/// <summary>Raised when a published dashboard's data is refreshed. Does not change lifecycle status.</summary>
public sealed record DashboardRefreshedEvent(
    DashboardEventId EventId,
    DashboardId DashboardId,
    UtcTimestamp RefreshedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IDashboardDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "DashboardRefreshed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => RefreshedAtUtc;
}

/// <summary>Raised when a published dashboard is deprecated (Active -&gt; Completed).</summary>
public sealed record DashboardDeprecatedEvent(
    DashboardEventId EventId,
    DashboardId DashboardId,
    UserId DeprecatedByUserId,
    UtcTimestamp DeprecatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IDashboardDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "DashboardDeprecated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => DeprecatedAtUtc;
}

/// <summary>Raised when a deprecated dashboard is formally closed (Completed -&gt; Closed).</summary>
public sealed record DashboardClosedEvent(
    DashboardEventId EventId,
    DashboardId DashboardId,
    UserId ClosedByUserId,
    UtcTimestamp ClosedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IDashboardDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "DashboardClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ClosedAtUtc;
}

/// <summary>Raised when a closed dashboard is archived for long-term retention (Closed -&gt; Archived).</summary>
public sealed record DashboardArchivedEvent(
    DashboardEventId EventId,
    DashboardId DashboardId,
    UtcTimestamp ArchivedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IDashboardDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "DashboardArchived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ArchivedAtUtc;
}
