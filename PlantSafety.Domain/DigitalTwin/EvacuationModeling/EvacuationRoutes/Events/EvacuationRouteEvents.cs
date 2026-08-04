// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationRoutes
//  Layer:          Domain
//  File:           EvacuationRouteEvents.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain events raised across the real EvacuationRoute lifecycle: definition,
//                  obstruction, clearance, and retirement. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Events;

/// <summary>Raised when a new evacuation route is defined against a validated Digital Twin model version.</summary>
public sealed record EvacuationRouteDefinedEvent(
    EvacuationRouteEventId EventId,
    EvacuationRouteId EvacuationRouteId,
    EvacuationZoneId ZoneId,
    string TwinModelVersion,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEvacuationRouteDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EvacuationRouteDefined";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when an obstruction is recorded against an evacuation route.</summary>
public sealed record EvacuationRouteObstructedEvent(
    EvacuationRouteEventId EventId,
    EvacuationRouteId EvacuationRouteId,
    RouteObstructionId RouteObstructionId,
    ObstructionSeverity Severity,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEvacuationRouteDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EvacuationRouteObstructed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when an obstruction on an evacuation route is cleared.</summary>
public sealed record EvacuationRouteClearedEvent(
    EvacuationRouteEventId EventId,
    EvacuationRouteId EvacuationRouteId,
    RouteObstructionId RouteObstructionId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEvacuationRouteDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EvacuationRouteCleared";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when an evacuation route is retired.</summary>
public sealed record EvacuationRouteRetiredEvent(
    EvacuationRouteEventId EventId,
    EvacuationRouteId EvacuationRouteId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEvacuationRouteDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EvacuationRouteRetired";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}
