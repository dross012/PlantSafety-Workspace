// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability
//  Layer:          Domain
//  File:           SustainabilityLifecycleEvents.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Domain events for the Sustainability aggregate's full lifecycle
//                  (Activate/Complete/Close/Archive), raised alongside the pre-existing
//                  SustainabilityCreatedEvent. Phase 4 full build-out -- these transitions
//                  previously did not exist on the aggregate at all. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Sustainability.Events;

/// <summary>Raised when a sustainability initiative is activated (Created -&gt; Active).</summary>
public sealed record SustainabilityActivatedEvent(
    Guid EventId,
    Guid SustainabilityId,
    Guid ActivatedByUserId,
    UtcTimestamp ActivatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ISustainabilityDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "SustainabilityActivated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ActivatedAtUtc;
}

/// <summary>Raised when an active sustainability initiative is completed (Active -&gt; Completed).</summary>
public sealed record SustainabilityCompletedEvent(
    Guid EventId,
    Guid SustainabilityId,
    Guid CompletedByUserId,
    UtcTimestamp CompletedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ISustainabilityDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "SustainabilityCompleted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CompletedAtUtc;
}

/// <summary>Raised when a completed sustainability initiative is formally closed (Completed -&gt; Closed).</summary>
public sealed record SustainabilityClosedEvent(
    Guid EventId,
    Guid SustainabilityId,
    Guid ClosedByUserId,
    UtcTimestamp ClosedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ISustainabilityDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "SustainabilityClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ClosedAtUtc;
}

/// <summary>Raised when a closed sustainability initiative is archived for retention (Closed -&gt; Archived).</summary>
public sealed record SustainabilityArchivedEvent(
    Guid EventId,
    Guid SustainabilityId,
    UtcTimestamp ArchivedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ISustainabilityDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "SustainabilityArchived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ArchivedAtUtc;
}
