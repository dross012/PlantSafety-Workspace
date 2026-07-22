// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.SafetyValvesRelays
//  Layer:          Domain
//  File:           SafetyValvesRelaysCreatedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain events raised across the real safety valve/relay component lifecycle:
//                  installation, inspection scheduling, reliable/fault outcome, repair, and retirement.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.SafetyValvesRelays.Events;

/// <summary>Raised when a new safety component is installed.</summary>
public sealed record SafetyComponentInstalledEvent(
    SafetyValvesRelaysEventId EventId,
    SafetyValvesRelaysId SafetyValvesRelaysId,
    SafetyComponentType ComponentType,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ISafetyValvesRelaysDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "SafetyComponentInstalled";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the next reliability inspection is scheduled.</summary>
public sealed record SafetyComponentInspectionScheduledEvent(
    SafetyValvesRelaysEventId EventId,
    SafetyValvesRelaysId SafetyValvesRelaysId,
    UtcTimestamp DueAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ISafetyValvesRelaysDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "SafetyComponentInspectionScheduled";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => DueAtUtc;
}

/// <summary>Raised when an inspection confirms the component is reliable.</summary>
public sealed record SafetyComponentReliableEvent(
    SafetyValvesRelaysEventId EventId,
    SafetyValvesRelaysId SafetyValvesRelaysId,
    decimal ReliabilityScore,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ISafetyValvesRelaysDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "SafetyComponentReliable";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when an inspection detects a fault.</summary>
public sealed record SafetyComponentFaultDetectedEvent(
    SafetyValvesRelaysEventId EventId,
    SafetyValvesRelaysId SafetyValvesRelaysId,
    int FaultCount,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ISafetyValvesRelaysDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "SafetyComponentFaultDetected";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when repair begins following a detected fault.</summary>
public sealed record SafetyComponentRepairStartedEvent(
    SafetyValvesRelaysEventId EventId,
    SafetyValvesRelaysId SafetyValvesRelaysId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ISafetyValvesRelaysDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "SafetyComponentRepairStarted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the component is retired.</summary>
public sealed record SafetyComponentRetiredEvent(
    SafetyValvesRelaysEventId EventId,
    SafetyValvesRelaysId SafetyValvesRelaysId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ISafetyValvesRelaysDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "SafetyComponentRetired";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}
