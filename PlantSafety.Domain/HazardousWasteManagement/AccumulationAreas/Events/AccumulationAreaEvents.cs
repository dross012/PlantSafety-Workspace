// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.AccumulationAreas
//  Layer:          Domain
//  File:           AccumulationAreaEvents.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain events raised across the AccumulationArea lifecycle: registration,
//                  time-limit-approaching escalation, and deactivation.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.AccumulationAreas.ValueObjects.Ids;

namespace PlantSafety.Domain.HazardousWasteManagement.AccumulationAreas.Events;

/// <summary>Raised when a new accumulation area is registered.</summary>
public sealed record AccumulationAreaRegisteredEvent(
    AccumulationAreaEventId EventId,
    AccumulationAreaId AccumulationAreaId,
    int TimeLimitDays,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAccumulationAreaDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AccumulationAreaRegistered";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when a container within this area is approaching its regulatory accumulation
/// time limit. Downstream Scheduling/RiskManagement consumers use this for escalation.</summary>
public sealed record AccumulationTimeLimitApproachingEvent(
    AccumulationAreaEventId EventId,
    AccumulationAreaId AccumulationAreaId,
    Guid WasteContainerId,
    DateTime DeadlineUtc,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAccumulationAreaDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AccumulationTimeLimitApproaching";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when an accumulation area is deactivated.</summary>
public sealed record AccumulationAreaDeactivatedEvent(
    AccumulationAreaEventId EventId,
    AccumulationAreaId AccumulationAreaId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAccumulationAreaDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AccumulationAreaDeactivated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}
