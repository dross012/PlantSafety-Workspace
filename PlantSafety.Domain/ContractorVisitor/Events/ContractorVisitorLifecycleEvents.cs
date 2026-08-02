// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorVisitor
//  Layer:          Domain
//  File:           ContractorVisitorLifecycleEvents.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Domain events for the ContractorVisitor aggregate's full lifecycle
//                  (Activate/CheckIn/Complete/Close/Archive), raised alongside the pre-existing
//                  ContractorVisitorCreatedEvent. Phase 4 full build-out. Mirrors
//                  PlantSafety.Domain.Compliance.Auditing.Events.AuditingLifecycleEvents shape.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorVisitor.Events;

/// <summary>Raised when a ContractorVisitor record is activated -- approved/scheduled (Created -&gt; Active).</summary>
public sealed record ContractorVisitorActivatedEvent(
    Guid EventId,
    Guid ContractorVisitorId,
    Guid PerformedBy,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IContractorVisitorDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ContractorVisitorActivated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a contractor/visitor's actual arrival is recorded (same-status action while Active).</summary>
public sealed record ContractorVisitorCheckedInEvent(
    Guid EventId,
    Guid ContractorVisitorId,
    Guid PerformedBy,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IContractorVisitorDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ContractorVisitorCheckedIn";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a ContractorVisitor record is completed (Active -&gt; Completed).</summary>
public sealed record ContractorVisitorCompletedEvent(
    Guid EventId,
    Guid ContractorVisitorId,
    Guid PerformedBy,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IContractorVisitorDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ContractorVisitorCompleted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a ContractorVisitor record is closed (Completed -&gt; Closed).</summary>
public sealed record ContractorVisitorClosedEvent(
    Guid EventId,
    Guid ContractorVisitorId,
    Guid PerformedBy,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IContractorVisitorDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ContractorVisitorClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a ContractorVisitor record is archived (Closed -&gt; Archived).</summary>
public sealed record ContractorVisitorArchivedEvent(
    Guid EventId,
    Guid ContractorVisitorId,
    Guid PerformedBy,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IContractorVisitorDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ContractorVisitorArchived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}
