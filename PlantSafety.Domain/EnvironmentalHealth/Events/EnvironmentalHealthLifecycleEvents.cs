// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth
//  Layer:          Domain
//  File:           EnvironmentalHealthLifecycleEvents.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Domain events for the EnvironmentalHealth aggregate's full lifecycle
//                  (Activate/Complete/Close/Archive), raised alongside the pre-existing
//                  EnvironmentalHealthCreatedEvent. Phase 4 full build-out -- these transitions
//                  previously did not exist on the aggregate at all. Mirrors
//                  PlantSafety.Domain.Compliance.Auditing.Events.AuditingLifecycleEvents.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.Events;

/// <summary>Raised when an EnvironmentalHealth record is activated (Created -&gt; Active).</summary>
public sealed record EnvironmentalHealthActivatedEvent(
    Guid EventId,
    Guid EnvironmentalHealthId,
    UserId ActorUserId,
    UtcTimestamp ActivatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEnvironmentalHealthDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EnvironmentalHealthActivated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ActivatedAtUtc;
}

/// <summary>Raised when an EnvironmentalHealth record is completed (Active -&gt; Completed).</summary>
public sealed record EnvironmentalHealthCompletedEvent(
    Guid EventId,
    Guid EnvironmentalHealthId,
    UserId ActorUserId,
    UtcTimestamp CompletedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEnvironmentalHealthDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EnvironmentalHealthCompleted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CompletedAtUtc;
}

/// <summary>Raised when an EnvironmentalHealth record is closed (Completed -&gt; Closed).</summary>
public sealed record EnvironmentalHealthClosedEvent(
    Guid EventId,
    Guid EnvironmentalHealthId,
    UserId ActorUserId,
    UtcTimestamp ClosedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEnvironmentalHealthDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EnvironmentalHealthClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ClosedAtUtc;
}

/// <summary>Raised when an EnvironmentalHealth record is archived (Closed -&gt; Archived).</summary>
public sealed record EnvironmentalHealthArchivedEvent(
    Guid EventId,
    Guid EnvironmentalHealthId,
    UserId ActorUserId,
    UtcTimestamp ArchivedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEnvironmentalHealthDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EnvironmentalHealthArchived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ArchivedAtUtc;
}
