// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.HazardIsolationSequencing
//  Layer:          Domain
//  File:           HazardIsolationSequencingLifecycleEvents.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Domain events for the HazardIsolationSequencing aggregate's lifecycle
//                  (Activate/FlagConflict/Complete/Close/Archive), raised alongside the
//                  pre-existing HazardIsolationSequencingCreatedEvent. Phase 4 full build-out --
//                  these transitions previously did not exist on the aggregate (only a Create
//                  factory did, though HazardIsolationSequencingLifecycleRules.Can* guards and the
//                  Created/Active/Completed/Closed/Archived status enum were already scaffolded
//                  and unused).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.HazardIsolationSequencing.Events;

/// <summary>Raised when the isolation window is activated (Created -&gt; Active).</summary>
public sealed record HazardIsolationSequencingActivatedEvent(
    HazardIsolationSequencingEventId EventId,
    HazardIsolationSequencingId HazardIsolationSequencingId,
    UserId ActivatedBy,
    UtcTimestamp ActivatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IHazardIsolationSequencingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "HazardIsolationSequencingActivated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ActivatedAtUtc;
}

/// <summary>Raised when an incompatible-task conflict is flagged within an active isolation window. Does not change lifecycle status.</summary>
public sealed record HazardIsolationSequencingConflictFlaggedEvent(
    HazardIsolationSequencingEventId EventId,
    HazardIsolationSequencingId HazardIsolationSequencingId,
    UserId FlaggedBy,
    string Reason,
    UtcTimestamp FlaggedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IHazardIsolationSequencingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "HazardIsolationSequencingConflictFlagged";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => FlaggedAtUtc;
}

/// <summary>Raised when the isolation window ends without unresolved conflict (Active -&gt; Completed).</summary>
public sealed record HazardIsolationSequencingCompletedEvent(
    HazardIsolationSequencingEventId EventId,
    HazardIsolationSequencingId HazardIsolationSequencingId,
    UserId CompletedBy,
    UtcTimestamp CompletedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IHazardIsolationSequencingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "HazardIsolationSequencingCompleted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CompletedAtUtc;
}

/// <summary>Raised when a completed isolation record is closed (Completed -&gt; Closed).</summary>
public sealed record HazardIsolationSequencingClosedEvent(
    HazardIsolationSequencingEventId EventId,
    HazardIsolationSequencingId HazardIsolationSequencingId,
    UserId ClosedBy,
    UtcTimestamp ClosedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IHazardIsolationSequencingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "HazardIsolationSequencingClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ClosedAtUtc;
}

/// <summary>Raised when a closed isolation record is archived for compliance retention (Closed -&gt; Archived).</summary>
public sealed record HazardIsolationSequencingArchivedEvent(
    HazardIsolationSequencingEventId EventId,
    HazardIsolationSequencingId HazardIsolationSequencingId,
    UtcTimestamp ArchivedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IHazardIsolationSequencingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "HazardIsolationSequencingArchived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ArchivedAtUtc;
}
