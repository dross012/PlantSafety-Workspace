// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.HumanRobotInteractionEvent
//  Layer:          Domain
//  File:           HumanRobotInteractionEventRecordedEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain events raised across the HumanRobotInteractionEvent lifecycle: recording,
//                  review start, and closure. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.HumanRobotInteractionEvent.Events;

/// <summary>Raised when a new human-robot interaction event is recorded.</summary>
public sealed record HumanRobotInteractionEventRecordedEvent(
    HumanRobotInteractionEventEventId EventId,
    HumanRobotInteractionEventId HumanRobotInteractionEventId,
    Guid AutonomousVehicleAssetId,
    HumanRobotInteractionType InteractionType,
    InteractionSeverityLevel SeverityLevel,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IHumanRobotInteractionEventDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "HumanRobotInteractionEventRecorded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when safety review of an interaction event begins.</summary>
public sealed record HumanRobotInteractionEventReviewStartedEvent(
    HumanRobotInteractionEventEventId EventId,
    HumanRobotInteractionEventId HumanRobotInteractionEventId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IHumanRobotInteractionEventDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "HumanRobotInteractionEventReviewStarted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when an interaction event review is closed.</summary>
public sealed record HumanRobotInteractionEventClosedEvent(
    HumanRobotInteractionEventEventId EventId,
    HumanRobotInteractionEventId HumanRobotInteractionEventId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IHumanRobotInteractionEventDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "HumanRobotInteractionEventClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}
