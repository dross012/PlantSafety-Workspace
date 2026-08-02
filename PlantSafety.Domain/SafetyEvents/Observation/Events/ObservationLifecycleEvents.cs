// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.Observation
//  Layer:          Domain
//  File:           ObservationLifecycleEvents.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Domain events for the Observation aggregate's lifecycle (Activate/Complete/
//                  Close/Archive), raised alongside the pre-existing ObservationCreatedEvent.
//                  Phase 4 full build-out -- this aggregate previously had only a Create factory.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.Observation.Events;

/// <summary>Raised when follow-up tracking begins for an observation requiring corrective action (Created -&gt; Active).</summary>
public sealed record ObservationActivatedEvent(
    ObservationEventId EventId,
    ObservationId ObservationId,
    UserId ActivatedBy,
    UtcTimestamp ActivatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IObservationDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ObservationActivated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ActivatedAtUtc;
}

/// <summary>Raised when the required follow-up/corrective action is completed (Active -&gt; Completed).</summary>
public sealed record ObservationCompletedEvent(
    ObservationEventId EventId,
    ObservationId ObservationId,
    UserId CompletedBy,
    string? ResolutionNotes,
    UtcTimestamp CompletedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IObservationDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ObservationCompleted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CompletedAtUtc;
}

/// <summary>Raised when a completed observation record is closed (Completed -&gt; Closed).</summary>
public sealed record ObservationClosedEvent(
    ObservationEventId EventId,
    ObservationId ObservationId,
    UserId ClosedBy,
    UtcTimestamp ClosedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IObservationDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ObservationClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ClosedAtUtc;
}

/// <summary>Raised when a closed observation record is archived for retention (Closed -&gt; Archived).</summary>
public sealed record ObservationArchivedEvent(
    ObservationEventId EventId,
    ObservationId ObservationId,
    UtcTimestamp ArchivedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IObservationDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ObservationArchived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ArchivedAtUtc;
}
