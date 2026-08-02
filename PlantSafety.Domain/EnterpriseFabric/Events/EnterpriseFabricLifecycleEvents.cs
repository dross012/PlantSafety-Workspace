// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnterpriseFabric
//  Layer:          Domain
//  File:           EnterpriseFabricLifecycleEvents.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Domain events for the EnterpriseFabric aggregate's full lifecycle
//                  (Activate/Complete/Close/Archive), raised alongside the pre-existing
//                  EnterpriseFabricCreatedEvent. Phase 4 full build-out -- this aggregate
//                  previously had only the Create factory. Uses the same Guid-based event
//                  envelope shape as the pre-existing EnterpriseFabricCreatedEvent (rather than
//                  introducing a new strong-typed EventId), for in-file consistency.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnterpriseFabric.Events;

/// <summary>Raised when an EnterpriseFabric record is activated (Created -&gt; Active).</summary>
public sealed record EnterpriseFabricActivatedEvent(
    Guid EventId,
    Guid EnterpriseFabricId,
    Guid ActivatedBy,
    UtcTimestamp ActivatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEnterpriseFabricDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EnterpriseFabricActivated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ActivatedAtUtc;
}

/// <summary>Raised when an EnterpriseFabric record is completed (Active -&gt; Completed).</summary>
public sealed record EnterpriseFabricCompletedEvent(
    Guid EventId,
    Guid EnterpriseFabricId,
    Guid CompletedBy,
    UtcTimestamp CompletedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEnterpriseFabricDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EnterpriseFabricCompleted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CompletedAtUtc;
}

/// <summary>Raised when an EnterpriseFabric record is closed (Completed -&gt; Closed).</summary>
public sealed record EnterpriseFabricClosedEvent(
    Guid EventId,
    Guid EnterpriseFabricId,
    Guid ClosedBy,
    UtcTimestamp ClosedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEnterpriseFabricDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EnterpriseFabricClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ClosedAtUtc;
}

/// <summary>Raised when an EnterpriseFabric record is archived (Closed -&gt; Archived).</summary>
public sealed record EnterpriseFabricArchivedEvent(
    Guid EventId,
    Guid EnterpriseFabricId,
    UtcTimestamp ArchivedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEnterpriseFabricDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EnterpriseFabricArchived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ArchivedAtUtc;
}
