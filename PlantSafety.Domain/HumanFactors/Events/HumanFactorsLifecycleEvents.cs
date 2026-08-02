// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors
//  Layer:          Domain
//  File:           HumanFactorsLifecycleEvents.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Domain events for the HumanFactors aggregate's full lifecycle
//                  (Activate/Complete/Close/Archive), raised alongside the pre-existing
//                  HumanFactorsCreatedEvent. Phase 4 full build-out -- these transitions
//                  previously did not exist on the aggregate at all. Mirrors
//                  PlantSafety.Domain.Compliance.Auditing.Events.AuditingLifecycleEvents.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HumanFactors.Events;

/// <summary>Raised when a HumanFactors record is activated (Created -&gt; Active).</summary>
public sealed record HumanFactorsActivatedEvent(
    Guid EventId,
    Guid HumanFactorsId,
    UserId ActorUserId,
    UtcTimestamp ActivatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IHumanFactorsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "HumanFactorsActivated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ActivatedAtUtc;
}

/// <summary>Raised when a HumanFactors record is completed (Active -&gt; Completed).</summary>
public sealed record HumanFactorsCompletedEvent(
    Guid EventId,
    Guid HumanFactorsId,
    UserId ActorUserId,
    UtcTimestamp CompletedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IHumanFactorsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "HumanFactorsCompleted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CompletedAtUtc;
}

/// <summary>Raised when a HumanFactors record is closed (Completed -&gt; Closed).</summary>
public sealed record HumanFactorsClosedEvent(
    Guid EventId,
    Guid HumanFactorsId,
    UserId ActorUserId,
    UtcTimestamp ClosedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IHumanFactorsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "HumanFactorsClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ClosedAtUtc;
}

/// <summary>Raised when a HumanFactors record is archived (Closed -&gt; Archived).</summary>
public sealed record HumanFactorsArchivedEvent(
    Guid EventId,
    Guid HumanFactorsId,
    UserId ActorUserId,
    UtcTimestamp ArchivedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IHumanFactorsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "HumanFactorsArchived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ArchivedAtUtc;
}
