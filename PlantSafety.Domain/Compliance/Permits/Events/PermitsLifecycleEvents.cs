// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Permits
//  Layer:          Domain
//  File:           PermitsLifecycleEvents.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Domain events for the Permits aggregate's Full Permit-to-Work lifecycle
//                  (Approve/Reject/Complete/Expire/Close/Archive), raised alongside the
//                  pre-existing PermitsCreatedEvent. Phase 4 full build-out -- these transitions
//                  previously did not exist on the aggregate at all.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Permits.Events;

/// <summary>Raised when a Permits request is approved and issued (Created -&gt; Active).</summary>
public sealed record PermitsApprovedEvent(
    PermitsEventId EventId,
    PermitsId PermitsId,
    UserId ApprovedByUserId,
    UtcTimestamp ApprovedAtUtc,
    UtcTimestamp? ExpiresAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPermitsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PermitsApproved";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ApprovedAtUtc;
}

/// <summary>Raised when a Permits request is declined before approval (Created -&gt; Rejected).</summary>
public sealed record PermitsRejectedEvent(
    PermitsEventId EventId,
    PermitsId PermitsId,
    UserId RejectedByUserId,
    string Reason,
    UtcTimestamp RejectedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPermitsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PermitsRejected";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => RejectedAtUtc;
}

/// <summary>Raised when the permitted work has been completed (Active -&gt; Completed).</summary>
public sealed record PermitsCompletedEvent(
    PermitsEventId EventId,
    PermitsId PermitsId,
    UtcTimestamp CompletedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPermitsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PermitsCompleted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CompletedAtUtc;
}

/// <summary>Raised when an issued permit passes its expiry window without being completed/closed (Active -&gt; Expired).</summary>
public sealed record PermitsExpiredEvent(
    PermitsEventId EventId,
    PermitsId PermitsId,
    UtcTimestamp ExpiredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPermitsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PermitsExpired";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ExpiredAtUtc;
}

/// <summary>Raised when a completed permit is formally closed (Completed -&gt; Closed).</summary>
public sealed record PermitsClosedEvent(
    PermitsEventId EventId,
    PermitsId PermitsId,
    UtcTimestamp ClosedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPermitsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PermitsClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ClosedAtUtc;
}

/// <summary>Raised when a closed permit is archived for compliance retention (Closed -&gt; Archived).</summary>
public sealed record PermitsArchivedEvent(
    PermitsEventId EventId,
    PermitsId PermitsId,
    UtcTimestamp ArchivedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPermitsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PermitsArchived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ArchivedAtUtc;
}
