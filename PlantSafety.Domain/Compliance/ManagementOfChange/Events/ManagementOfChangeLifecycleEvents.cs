// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.ManagementOfChange
//  Layer:          Domain
//  File:           ManagementOfChangeLifecycleEvents.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Domain events for the ManagementOfChange aggregate's full lifecycle
//                  (Approve/Implement/Close/Archive), raised alongside the pre-existing
//                  ManagementOfChangeCreatedEvent. Phase 4 full build-out -- these transitions
//                  previously did not exist on the aggregate at all. Mirrors
//                  PlantSafety.Domain.Compliance.Permits.Events.PermitsLifecycleEvents.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.ManagementOfChange.Events;

/// <summary>Raised when a ManagementOfChange request is approved and made active (Created -&gt; Active).</summary>
public sealed record ManagementOfChangeApprovedEvent(
    ManagementOfChangeEventId EventId,
    ManagementOfChangeId ManagementOfChangeId,
    UserId ApprovedByUserId,
    UtcTimestamp ApprovedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IManagementOfChangeDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ManagementOfChangeApproved";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ApprovedAtUtc;
}

/// <summary>Raised when an approved change is implemented (Active -&gt; Completed).</summary>
public sealed record ManagementOfChangeImplementedEvent(
    ManagementOfChangeEventId EventId,
    ManagementOfChangeId ManagementOfChangeId,
    UserId ImplementedByUserId,
    UtcTimestamp ImplementationAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IManagementOfChangeDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ManagementOfChangeImplemented";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ImplementationAtUtc;
}

/// <summary>Raised when an implemented change is formally closed (Completed -&gt; Closed).</summary>
public sealed record ManagementOfChangeClosedEvent(
    ManagementOfChangeEventId EventId,
    ManagementOfChangeId ManagementOfChangeId,
    UserId ClosedByUserId,
    UtcTimestamp ClosedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IManagementOfChangeDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ManagementOfChangeClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ClosedAtUtc;
}

/// <summary>Raised when a closed change is archived for compliance retention (Closed -&gt; Archived).</summary>
public sealed record ManagementOfChangeArchivedEvent(
    ManagementOfChangeEventId EventId,
    ManagementOfChangeId ManagementOfChangeId,
    UtcTimestamp ArchivedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IManagementOfChangeDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ManagementOfChangeArchived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ArchivedAtUtc;
}
