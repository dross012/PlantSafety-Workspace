// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         WorkerWellBeing
//  Layer:          Domain
//  File:           WorkerWellBeingLifecycleEvents.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Domain events for the WorkerWellBeing aggregate's full lifecycle
//                  (Activate/Complete/Close/Archive), raised alongside the pre-existing
//                  WorkerWellBeingCreatedEvent. Phase 4 full build-out -- these transitions
//                  previously did not exist on the aggregate at all. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.WorkerWellBeing.Events;

/// <summary>Raised when a worker wellbeing check-in record is activated (Created -&gt; Active).</summary>
public sealed record WorkerWellBeingActivatedEvent(
    Guid EventId,
    Guid WorkerWellBeingId,
    Guid ActivatedByUserId,
    UtcTimestamp ActivatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IWorkerWellBeingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "WorkerWellBeingActivated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ActivatedAtUtc;
}

/// <summary>Raised when an active worker wellbeing record is completed (Active -&gt; Completed).</summary>
public sealed record WorkerWellBeingCompletedEvent(
    Guid EventId,
    Guid WorkerWellBeingId,
    Guid CompletedByUserId,
    UtcTimestamp CompletedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IWorkerWellBeingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "WorkerWellBeingCompleted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CompletedAtUtc;
}

/// <summary>Raised when a completed worker wellbeing record is formally closed (Completed -&gt; Closed).</summary>
public sealed record WorkerWellBeingClosedEvent(
    Guid EventId,
    Guid WorkerWellBeingId,
    Guid ClosedByUserId,
    UtcTimestamp ClosedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IWorkerWellBeingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "WorkerWellBeingClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ClosedAtUtc;
}

/// <summary>Raised when a closed worker wellbeing record is archived for retention (Closed -&gt; Archived).</summary>
public sealed record WorkerWellBeingArchivedEvent(
    Guid EventId,
    Guid WorkerWellBeingId,
    UtcTimestamp ArchivedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IWorkerWellBeingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "WorkerWellBeingArchived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ArchivedAtUtc;
}
