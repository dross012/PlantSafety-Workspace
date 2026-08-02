// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Policy
//  Layer:          Domain
//  File:           PolicyLifecycleEvents.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Domain events for the Policy aggregate's full lifecycle
//                  (Approve/RecordAcknowledgment/Supersede/Close/Archive), raised alongside the
//                  pre-existing PolicyCreatedEvent. Phase 4 full build-out -- these transitions
//                  previously did not exist on the aggregate at all. Mirrors
//                  PlantSafety.Domain.Compliance.Permits.Events.PermitsLifecycleEvents.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Policy.Events;

/// <summary>Raised when a Policy is approved and becomes effective (Created -&gt; Active).</summary>
public sealed record PolicyApprovedEvent(
    PolicyEventId EventId,
    PolicyId PolicyId,
    UserId ApprovedByUserId,
    UtcTimestamp EffectiveAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPolicyDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PolicyApproved";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => EffectiveAtUtc;
}

/// <summary>Raised when a person acknowledges an active Policy. Does not change lifecycle status.</summary>
public sealed record PolicyAcknowledgedEvent(
    PolicyEventId EventId,
    PolicyId PolicyId,
    UserId AcknowledgedByUserId,
    UtcTimestamp AcknowledgedAtUtc,
    int AcknowledgmentCount,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPolicyDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PolicyAcknowledged";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => AcknowledgedAtUtc;
}

/// <summary>Raised when an active Policy is superseded by a newer revision (Active -&gt; Completed).</summary>
public sealed record PolicySupersededEvent(
    PolicyEventId EventId,
    PolicyId PolicyId,
    UserId SupersededByUserId,
    UtcTimestamp SupersededAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPolicyDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PolicySuperseded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => SupersededAtUtc;
}

/// <summary>Raised when a superseded Policy is formally closed (Completed -&gt; Closed).</summary>
public sealed record PolicyClosedEvent(
    PolicyEventId EventId,
    PolicyId PolicyId,
    UserId ClosedByUserId,
    UtcTimestamp ClosedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPolicyDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PolicyClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ClosedAtUtc;
}

/// <summary>Raised when a closed Policy is archived for compliance retention (Closed -&gt; Archived).</summary>
public sealed record PolicyArchivedEvent(
    PolicyEventId EventId,
    PolicyId PolicyId,
    UtcTimestamp ArchivedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPolicyDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PolicyArchived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ArchivedAtUtc;
}
