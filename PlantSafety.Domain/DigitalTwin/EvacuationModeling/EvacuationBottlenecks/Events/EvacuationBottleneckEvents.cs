// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationBottlenecks
//  Layer:          Domain
//  File:           EvacuationBottleneckEvents.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain events raised across the real EvacuationBottleneck lifecycle: detection,
//                  recurrence, and mitigation. EvacuationBottleneckDetected escalates via
//                  AlertRoutingService (risk hook). Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Events;

/// <summary>Raised when a new evacuation bottleneck is detected from a completed egress-simulation run.</summary>
public sealed record EvacuationBottleneckDetectedEvent(
    EvacuationBottleneckEventId EventId,
    EvacuationBottleneckId EvacuationBottleneckId,
    EvacuationRouteId RouteId,
    EgressSimulationRunId SimulationRunId,
    int DelaySeconds,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEvacuationBottleneckDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EvacuationBottleneckDetected";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a previously detected bottleneck recurs on a subsequent simulation run.</summary>
public sealed record EvacuationBottleneckRecurredEvent(
    EvacuationBottleneckEventId EventId,
    EvacuationBottleneckId EvacuationBottleneckId,
    EgressSimulationRunId SimulationRunId,
    int RecurrenceCount,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEvacuationBottleneckDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EvacuationBottleneckRecurred";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when an evacuation bottleneck is mitigated.</summary>
public sealed record EvacuationBottleneckMitigatedEvent(
    EvacuationBottleneckEventId EventId,
    EvacuationBottleneckId EvacuationBottleneckId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEvacuationBottleneckDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EvacuationBottleneckMitigated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}
