// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.EmergencyStopDevices
//  Layer:          Domain
//  File:           EmergencyStopDevicesCreatedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain events raised across the real E-Stop device lifecycle: installation, test
//                  scheduling, pass/fail outcome, repair, and retirement.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.EmergencyStopDevices.Events;

/// <summary>Raised when a new E-Stop device is installed.</summary>
public sealed record EmergencyStopDeviceInstalledEvent(
    EmergencyStopDevicesEventId EventId,
    EmergencyStopDevicesId EmergencyStopDevicesId,
    EStopDeviceType DeviceType,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEmergencyStopDevicesDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EmergencyStopDeviceInstalled";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the next functional test is scheduled.</summary>
public sealed record EmergencyStopDeviceTestScheduledEvent(
    EmergencyStopDevicesEventId EventId,
    EmergencyStopDevicesId EmergencyStopDevicesId,
    UtcTimestamp DueAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEmergencyStopDevicesDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EmergencyStopDeviceTestScheduled";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => DueAtUtc;
}

/// <summary>Raised when a functional test passes.</summary>
public sealed record EmergencyStopDeviceTestPassedEvent(
    EmergencyStopDevicesEventId EventId,
    EmergencyStopDevicesId EmergencyStopDevicesId,
    decimal ResponseTimeScore,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEmergencyStopDevicesDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EmergencyStopDeviceTestPassed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when a functional test detects a failure.</summary>
public sealed record EmergencyStopDeviceFailureDetectedEvent(
    EmergencyStopDevicesEventId EventId,
    EmergencyStopDevicesId EmergencyStopDevicesId,
    int FailureCount,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEmergencyStopDevicesDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EmergencyStopDeviceFailureDetected";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when repair begins following a test failure.</summary>
public sealed record EmergencyStopDeviceRepairStartedEvent(
    EmergencyStopDevicesEventId EventId,
    EmergencyStopDevicesId EmergencyStopDevicesId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEmergencyStopDevicesDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EmergencyStopDeviceRepairStarted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the device is retired.</summary>
public sealed record EmergencyStopDeviceRetiredEvent(
    EmergencyStopDevicesEventId EventId,
    EmergencyStopDevicesId EmergencyStopDevicesId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEmergencyStopDevicesDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EmergencyStopDeviceRetired";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}
