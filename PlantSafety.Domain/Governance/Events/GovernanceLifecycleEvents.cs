// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Governance
//  Layer:          Domain
//  File:           GovernanceLifecycleEvents.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Domain events for the Governance aggregate's full lifecycle
//                  (Activate/Complete/Close/Archive), raised alongside the pre-existing
//                  GovernanceCreatedEvent. Phase 4 full build-out -- these transitions previously
//                  did not exist on the aggregate at all. Mirrors
//                  PlantSafety.Domain.Compliance.Auditing.Events.AuditingLifecycleEvents.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Governance.Events;

/// <summary>Raised when a Governance record is activated (Created -&gt; Active).</summary>
public sealed record GovernanceActivatedEvent(
    Guid EventId,
    Guid GovernanceId,
    UserId ActorUserId,
    UtcTimestamp ActivatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IGovernanceDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "GovernanceActivated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ActivatedAtUtc;
}

/// <summary>Raised when a Governance record is completed (Active -&gt; Completed).</summary>
public sealed record GovernanceCompletedEvent(
    Guid EventId,
    Guid GovernanceId,
    UserId ActorUserId,
    UtcTimestamp CompletedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IGovernanceDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "GovernanceCompleted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CompletedAtUtc;
}

/// <summary>Raised when a Governance record is closed (Completed -&gt; Closed).</summary>
public sealed record GovernanceClosedEvent(
    Guid EventId,
    Guid GovernanceId,
    UserId ActorUserId,
    UtcTimestamp ClosedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IGovernanceDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "GovernanceClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ClosedAtUtc;
}

/// <summary>Raised when a Governance record is archived (Closed -&gt; Archived).</summary>
public sealed record GovernanceArchivedEvent(
    Guid EventId,
    Guid GovernanceId,
    UserId ActorUserId,
    UtcTimestamp ArchivedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IGovernanceDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "GovernanceArchived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ArchivedAtUtc;
}
