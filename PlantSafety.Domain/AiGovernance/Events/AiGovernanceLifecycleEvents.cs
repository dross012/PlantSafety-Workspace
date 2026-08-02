// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AiGovernance
//  Layer:          Domain
//  File:           AiGovernanceLifecycleEvents.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Domain events for the AiGovernance aggregate's full lifecycle
//                  (Activate/Complete/Close/Archive), raised alongside the pre-existing
//                  AiGovernanceCreatedEvent. Phase 4 full build-out. Mirrors
//                  PlantSafety.Domain.Compliance.Auditing.Events.AuditingLifecycleEvents shape,
//                  using the same raw-Guid EventId/AiGovernanceId convention already established by
//                  AiGovernanceCreatedEvent. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AiGovernance.Events;

/// <summary>Raised when an AiGovernance record is activated (Created -&gt; Active).</summary>
public sealed record AiGovernanceActivatedEvent(
    Guid EventId,
    Guid AiGovernanceId,
    Guid PerformedBy,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAiGovernanceDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AiGovernanceActivated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when an AiGovernance record is completed (Active -&gt; Completed).</summary>
public sealed record AiGovernanceCompletedEvent(
    Guid EventId,
    Guid AiGovernanceId,
    Guid PerformedBy,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAiGovernanceDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AiGovernanceCompleted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when an AiGovernance record is closed (Completed -&gt; Closed).</summary>
public sealed record AiGovernanceClosedEvent(
    Guid EventId,
    Guid AiGovernanceId,
    Guid PerformedBy,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAiGovernanceDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AiGovernanceClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when an AiGovernance record is archived (Closed -&gt; Archived).</summary>
public sealed record AiGovernanceArchivedEvent(
    Guid EventId,
    Guid AiGovernanceId,
    Guid PerformedBy,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAiGovernanceDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AiGovernanceArchived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}
