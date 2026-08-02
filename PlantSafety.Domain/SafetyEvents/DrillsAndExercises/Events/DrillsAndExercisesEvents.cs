// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.DrillsAndExercises
//  Layer:          Domain
//  File:           DrillsAndExercisesEvents.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Domain events for the DrillsAndExercises aggregate's full lifecycle
//                  (Schedule/Conduct/Complete/Cancel/Close/Archive). Phase 4 full build-out --
//                  this aggregate did not exist before this pass (only a placeholder marker class
//                  and a read-only list query slice existed).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.DrillsAndExercises.Events;

/// <summary>Raised when a new drill/exercise is scheduled.</summary>
public sealed record DrillsAndExercisesScheduledEvent(
    DrillsAndExercisesEventId EventId,
    DrillsAndExercisesId DrillsAndExercisesId,
    Guid TenantId,
    Guid? SiteId,
    string DrillType,
    UtcTimestamp? ScheduledAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId,
    UtcTimestamp OccurredAtUtc
) : IDrillsAndExercisesDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "DrillsAndExercisesScheduled";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a scheduled drill/exercise is conducted (Scheduled -&gt; Conducted).</summary>
public sealed record DrillsAndExercisesConductedEvent(
    DrillsAndExercisesEventId EventId,
    DrillsAndExercisesId DrillsAndExercisesId,
    UserId ConductedBy,
    int ParticipantCount,
    UtcTimestamp ConductedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IDrillsAndExercisesDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "DrillsAndExercisesConducted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ConductedAtUtc;
}

/// <summary>Raised when a conducted drill/exercise is completed (Conducted -&gt; Completed).</summary>
public sealed record DrillsAndExercisesCompletedEvent(
    DrillsAndExercisesEventId EventId,
    DrillsAndExercisesId DrillsAndExercisesId,
    UserId CompletedBy,
    string? DebriefNotes,
    UtcTimestamp CompletedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IDrillsAndExercisesDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "DrillsAndExercisesCompleted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CompletedAtUtc;
}

/// <summary>Raised when a scheduled drill/exercise is cancelled before being conducted (Scheduled -&gt; Cancelled).</summary>
public sealed record DrillsAndExercisesCancelledEvent(
    DrillsAndExercisesEventId EventId,
    DrillsAndExercisesId DrillsAndExercisesId,
    UserId CancelledBy,
    string Reason,
    UtcTimestamp CancelledAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IDrillsAndExercisesDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "DrillsAndExercisesCancelled";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CancelledAtUtc;
}

/// <summary>Raised when a completed drill/exercise is formally closed (Completed -&gt; Closed).</summary>
public sealed record DrillsAndExercisesClosedEvent(
    DrillsAndExercisesEventId EventId,
    DrillsAndExercisesId DrillsAndExercisesId,
    UserId ClosedBy,
    UtcTimestamp ClosedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IDrillsAndExercisesDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "DrillsAndExercisesClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ClosedAtUtc;
}

/// <summary>Raised when a closed drill/exercise record is archived (Closed -&gt; Archived).</summary>
public sealed record DrillsAndExercisesArchivedEvent(
    DrillsAndExercisesEventId EventId,
    DrillsAndExercisesId DrillsAndExercisesId,
    UtcTimestamp ArchivedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IDrillsAndExercisesDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "DrillsAndExercisesArchived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ArchivedAtUtc;
}
