// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         OperationalSafety
//  Layer:          Domain
//  File:           OperationalSafetyLifecycleEvents.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Domain events for the OperationalSafety aggregate's full lifecycle
//                  (Activate/Complete/Close/Archive), raised alongside the pre-existing
//                  OperationalSafetyCreatedEvent. Phase 4 full build-out -- these transitions
//                  previously did not exist on the aggregate at all. Mirrors
//                  PlantSafety.Domain.Compliance.Auditing.Events.AuditingLifecycleEvents.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.OperationalSafety.Events;

/// <summary>Raised when an OperationalSafety record is activated (Created -&gt; Active).</summary>
public sealed record OperationalSafetyActivatedEvent(
    Guid EventId,
    Guid OperationalSafetyId,
    UserId ActorUserId,
    UtcTimestamp ActivatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IOperationalSafetyDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "OperationalSafetyActivated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ActivatedAtUtc;
}

/// <summary>Raised when an OperationalSafety record is completed (Active -&gt; Completed).</summary>
public sealed record OperationalSafetyCompletedEvent(
    Guid EventId,
    Guid OperationalSafetyId,
    UserId ActorUserId,
    UtcTimestamp CompletedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IOperationalSafetyDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "OperationalSafetyCompleted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CompletedAtUtc;
}

/// <summary>Raised when an OperationalSafety record is closed (Completed -&gt; Closed).</summary>
public sealed record OperationalSafetyClosedEvent(
    Guid EventId,
    Guid OperationalSafetyId,
    UserId ActorUserId,
    UtcTimestamp ClosedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IOperationalSafetyDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "OperationalSafetyClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ClosedAtUtc;
}

/// <summary>Raised when an OperationalSafety record is archived (Closed -&gt; Archived).</summary>
public sealed record OperationalSafetyArchivedEvent(
    Guid EventId,
    Guid OperationalSafetyId,
    UserId ActorUserId,
    UtcTimestamp ArchivedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IOperationalSafetyDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "OperationalSafetyArchived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ArchivedAtUtc;
}
