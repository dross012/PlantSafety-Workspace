// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.EnvironmentalHealthMonitoring
//  Layer:          Domain
//  File:           EnvironmentalHealthMonitoringCreatedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain events raised across the real environmental/health monitoring lifecycle:
//                  scheduling, measurement, exceedance detection, corrective action, and resolution.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.EnvironmentalHealthMonitoring.Events;

/// <summary>Raised when a new monitoring cycle is scheduled.</summary>
public sealed record EnvironmentalMonitoringScheduledEvent(
    EnvironmentalHealthMonitoringEventId EventId,
    EnvironmentalHealthMonitoringId EnvironmentalHealthMonitoringId,
    EnvironmentalMonitoringCategory Category,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEnvironmentalHealthMonitoringDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EnvironmentalMonitoringScheduled";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when a measurement is recorded within acceptable limits.</summary>
public sealed record EnvironmentalMonitoringMeasurementRecordedEvent(
    EnvironmentalHealthMonitoringEventId EventId,
    EnvironmentalHealthMonitoringId EnvironmentalHealthMonitoringId,
    decimal ExposureLevelScore,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEnvironmentalHealthMonitoringDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EnvironmentalMonitoringMeasurementRecorded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when a measurement exceeds the applicable exposure limit.</summary>
public sealed record EnvironmentalMonitoringExceedanceDetectedEvent(
    EnvironmentalHealthMonitoringEventId EventId,
    EnvironmentalHealthMonitoringId EnvironmentalHealthMonitoringId,
    decimal ExposureLevelScore,
    int ExceedanceCount,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEnvironmentalHealthMonitoringDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EnvironmentalMonitoringExceedanceDetected";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when corrective action is started to remediate an exceedance.</summary>
public sealed record EnvironmentalMonitoringCorrectiveActionStartedEvent(
    EnvironmentalHealthMonitoringEventId EventId,
    EnvironmentalHealthMonitoringId EnvironmentalHealthMonitoringId,
    string Description,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEnvironmentalHealthMonitoringDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EnvironmentalMonitoringCorrectiveActionStarted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when an exceedance is resolved following corrective action.</summary>
public sealed record EnvironmentalMonitoringExceedanceResolvedEvent(
    EnvironmentalHealthMonitoringEventId EventId,
    EnvironmentalHealthMonitoringId EnvironmentalHealthMonitoringId,
    decimal ReMeasuredScore,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEnvironmentalHealthMonitoringDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EnvironmentalMonitoringExceedanceResolved";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}
