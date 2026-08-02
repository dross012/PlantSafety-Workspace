// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AiSafetyIntelligence
//  Layer:          Domain
//  File:           AiSafetyIntelligenceLifecycleEvents.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Domain events for the AiSafetyIntelligence aggregate's full lifecycle
//                  (Activate/Complete/Close/Archive), raised alongside the pre-existing
//                  AiSafetyIntelligenceCreatedEvent. Phase 4 full build-out. Mirrors
//                  PlantSafety.Domain.Compliance.Auditing.Events.AuditingLifecycleEvents shape.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AiSafetyIntelligence.Events;

/// <summary>Raised when an AiSafetyIntelligence record is activated (Created -&gt; Active).</summary>
public sealed record AiSafetyIntelligenceActivatedEvent(
    Guid EventId,
    Guid AiSafetyIntelligenceId,
    Guid PerformedBy,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAiSafetyIntelligenceDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AiSafetyIntelligenceActivated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when an AiSafetyIntelligence record is completed (Active -&gt; Completed).</summary>
public sealed record AiSafetyIntelligenceCompletedEvent(
    Guid EventId,
    Guid AiSafetyIntelligenceId,
    Guid PerformedBy,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAiSafetyIntelligenceDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AiSafetyIntelligenceCompleted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when an AiSafetyIntelligence record is closed (Completed -&gt; Closed).</summary>
public sealed record AiSafetyIntelligenceClosedEvent(
    Guid EventId,
    Guid AiSafetyIntelligenceId,
    Guid PerformedBy,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAiSafetyIntelligenceDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AiSafetyIntelligenceClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when an AiSafetyIntelligence record is archived (Closed -&gt; Archived).</summary>
public sealed record AiSafetyIntelligenceArchivedEvent(
    Guid EventId,
    Guid AiSafetyIntelligenceId,
    Guid PerformedBy,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAiSafetyIntelligenceDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AiSafetyIntelligenceArchived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}
