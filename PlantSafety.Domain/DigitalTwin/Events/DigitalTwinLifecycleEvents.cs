// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin
//  Layer:          Domain
//  File:           DigitalTwinLifecycleEvents.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Domain events for the DigitalTwin aggregate's full lifecycle
//                  (Activate/RefreshSync/Complete/Close/Archive), raised alongside the pre-existing
//                  DigitalTwinCreatedEvent. Phase 4 full build-out. Mirrors
//                  PlantSafety.Domain.Compliance.Auditing.Events.AuditingLifecycleEvents shape.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.Events;

/// <summary>Raised when a DigitalTwin record is activated (Created -&gt; Active).</summary>
public sealed record DigitalTwinActivatedEvent(
    Guid EventId,
    Guid DigitalTwinId,
    Guid PerformedBy,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IDigitalTwinDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "DigitalTwinActivated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a digital twin's last-synchronized timestamp is refreshed (same-status action while Active).</summary>
public sealed record DigitalTwinSyncRefreshedEvent(
    Guid EventId,
    Guid DigitalTwinId,
    UtcTimestamp SyncedAtUtc,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IDigitalTwinDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "DigitalTwinSyncRefreshed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a DigitalTwin record is completed (Active -&gt; Completed).</summary>
public sealed record DigitalTwinCompletedEvent(
    Guid EventId,
    Guid DigitalTwinId,
    Guid PerformedBy,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IDigitalTwinDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "DigitalTwinCompleted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a DigitalTwin record is closed (Completed -&gt; Closed).</summary>
public sealed record DigitalTwinClosedEvent(
    Guid EventId,
    Guid DigitalTwinId,
    Guid PerformedBy,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IDigitalTwinDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "DigitalTwinClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a DigitalTwin record is archived (Closed -&gt; Archived).</summary>
public sealed record DigitalTwinArchivedEvent(
    Guid EventId,
    Guid DigitalTwinId,
    Guid PerformedBy,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IDigitalTwinDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "DigitalTwinArchived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}
