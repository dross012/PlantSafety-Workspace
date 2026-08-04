// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargePoint
//  Layer:          Domain
//  File:           DischargePointEvents.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain events raised across the DischargePoint lifecycle: registration,
//                  deactivation, reactivation, decommissioning. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Events;

/// <summary>Raised when a new discharge point is registered.</summary>
public sealed record DischargePointRegisteredEvent(
    DischargePointEventId EventId,
    DischargePointId DischargePointId,
    Guid DischargePermitId,
    string PointCode,
    OutfallType Outfall,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IDischargePointDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "DischargePointRegistered";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a discharge point is deactivated.</summary>
public sealed record DischargePointDeactivatedEvent(
    DischargePointEventId EventId,
    DischargePointId DischargePointId,
    string? Reason,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IDischargePointDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "DischargePointDeactivated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a discharge point is reactivated.</summary>
public sealed record DischargePointReactivatedEvent(
    DischargePointEventId EventId,
    DischargePointId DischargePointId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IDischargePointDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "DischargePointReactivated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a discharge point is decommissioned.</summary>
public sealed record DischargePointDecommissionedEvent(
    DischargePointEventId EventId,
    DischargePointId DischargePointId,
    string? Reason,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IDischargePointDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "DischargePointDecommissioned";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}