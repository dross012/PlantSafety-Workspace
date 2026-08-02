// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Procedures
//  Layer:          Domain
//  File:           ProceduresLifecycleEvents.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Domain events for the Procedures aggregate's full lifecycle
//                  (Approve/Supersede/Close/Archive), raised alongside the pre-existing
//                  ProceduresCreatedEvent. Phase 4 full build-out -- these transitions
//                  previously did not exist on the aggregate at all. Mirrors
//                  PlantSafety.Domain.Compliance.Permits.Events.PermitsLifecycleEvents.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Procedures.Events;

/// <summary>Raised when a Procedures document is approved and becomes effective (Created -&gt; Active).</summary>
public sealed record ProceduresApprovedEvent(
    ProceduresEventId EventId,
    ProceduresId ProceduresId,
    UserId ApprovedByUserId,
    UtcTimestamp EffectiveAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IProceduresDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ProceduresApproved";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => EffectiveAtUtc;
}

/// <summary>Raised when an active Procedures document is superseded by a newer revision (Active -&gt; Completed).</summary>
public sealed record ProceduresSupersededEvent(
    ProceduresEventId EventId,
    ProceduresId ProceduresId,
    UserId SupersededByUserId,
    UtcTimestamp SupersededAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IProceduresDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ProceduresSuperseded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => SupersededAtUtc;
}

/// <summary>Raised when a superseded Procedures document is formally closed (Completed -&gt; Closed).</summary>
public sealed record ProceduresClosedEvent(
    ProceduresEventId EventId,
    ProceduresId ProceduresId,
    UserId ClosedByUserId,
    UtcTimestamp ClosedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IProceduresDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ProceduresClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ClosedAtUtc;
}

/// <summary>Raised when a closed Procedures document is archived for compliance retention (Closed -&gt; Archived).</summary>
public sealed record ProceduresArchivedEvent(
    ProceduresEventId EventId,
    ProceduresId ProceduresId,
    UtcTimestamp ArchivedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IProceduresDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ProceduresArchived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ArchivedAtUtc;
}
