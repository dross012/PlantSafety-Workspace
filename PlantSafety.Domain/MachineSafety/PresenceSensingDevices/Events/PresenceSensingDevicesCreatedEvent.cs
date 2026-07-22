// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.PresenceSensingDevices
//  Layer:          Domain
//  File:           PresenceSensingDevicesCreatedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain events raised across the real presence-sensing device lifecycle:
//                  installation, calibration scheduling, calibrated/out-of-tolerance outcome, trigger
//                  events, and retirement.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.PresenceSensingDevices.Events;

/// <summary>Raised when a new presence-sensing device is installed.</summary>
public sealed record PresenceSensingDeviceInstalledEvent(
    PresenceSensingDevicesEventId EventId,
    PresenceSensingDevicesId PresenceSensingDevicesId,
    PresenceSensingDeviceType DeviceType,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPresenceSensingDevicesDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PresenceSensingDeviceInstalled";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the next calibration is scheduled.</summary>
public sealed record PresenceSensingDeviceCalibrationScheduledEvent(
    PresenceSensingDevicesEventId EventId,
    PresenceSensingDevicesId PresenceSensingDevicesId,
    UtcTimestamp DueAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPresenceSensingDevicesDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PresenceSensingDeviceCalibrationScheduled";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => DueAtUtc;
}

/// <summary>Raised when calibration confirms the device is within tolerance.</summary>
public sealed record PresenceSensingDeviceCalibratedEvent(
    PresenceSensingDevicesEventId EventId,
    PresenceSensingDevicesId PresenceSensingDevicesId,
    decimal DetectionAccuracyScore,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPresenceSensingDevicesDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PresenceSensingDeviceCalibrated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when calibration finds detection accuracy out of tolerance.</summary>
public sealed record PresenceSensingDeviceOutOfToleranceEvent(
    PresenceSensingDevicesEventId EventId,
    PresenceSensingDevicesId PresenceSensingDevicesId,
    decimal DetectionAccuracyScore,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPresenceSensingDevicesDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PresenceSensingDeviceOutOfTolerance";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the device's protective field is triggered.</summary>
public sealed record PresenceSensingDeviceTriggeredEvent(
    PresenceSensingDevicesEventId EventId,
    PresenceSensingDevicesId PresenceSensingDevicesId,
    int TriggerEventCount,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPresenceSensingDevicesDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PresenceSensingDeviceTriggered";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the device is retired.</summary>
public sealed record PresenceSensingDeviceRetiredEvent(
    PresenceSensingDevicesEventId EventId,
    PresenceSensingDevicesId PresenceSensingDevicesId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPresenceSensingDevicesDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PresenceSensingDeviceRetired";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}
