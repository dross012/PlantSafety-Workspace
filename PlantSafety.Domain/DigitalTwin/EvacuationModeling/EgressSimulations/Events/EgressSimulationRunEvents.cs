// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EgressSimulations
//  Layer:          Domain
//  File:           EgressSimulationRunEvents.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain events raised across the real EgressSimulationRun lifecycle: start,
//                  completion, failure, and drill correlation. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.Events;

/// <summary>Raised when a new egress-simulation run starts.</summary>
public sealed record EgressSimulationStartedEvent(
    EgressSimulationEventId EventId,
    EgressSimulationRunId EgressSimulationRunId,
    EvacuationZoneId ZoneId,
    EgressSimulationTrigger TriggerReason,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEgressSimulationRunDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EgressSimulationStarted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when an egress-simulation run completes with results.</summary>
public sealed record EgressSimulationCompletedEvent(
    EgressSimulationEventId EventId,
    EgressSimulationRunId EgressSimulationRunId,
    EvacuationZoneId ZoneId,
    int SimulatedEvacuationTimeSeconds,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEgressSimulationRunDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EgressSimulationCompleted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when an egress-simulation run fails before results could be computed.</summary>
public sealed record EgressSimulationFailedEvent(
    EgressSimulationEventId EventId,
    EgressSimulationRunId EgressSimulationRunId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEgressSimulationRunDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EgressSimulationFailed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when an egress-simulation run is correlated to an evacuation drill.</summary>
public sealed record EgressSimulationLinkedToDrillEvent(
    EgressSimulationEventId EventId,
    EgressSimulationRunId EgressSimulationRunId,
    Guid DrillId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEgressSimulationRunDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EgressSimulationLinkedToDrill";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}
