// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.LoadSecurementChecks
//  Layer:          Domain
//  File:           LoadSecurementCheckEvents.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain events raised across the LoadSecurementCheck lifecycle: recording, failure,
//                  and corrective action completion.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.LoadSecurementChecks.Events;

/// <summary>Raised when a load securement check is recorded.</summary>
public sealed record LoadSecurementCheckRecordedEvent(
    LoadSecurementCheckEventId EventId,
    LoadSecurementCheckId LoadSecurementCheckId,
    bool Passed,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ILoadSecurementCheckDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "LoadSecurementCheckRecorded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a load securement check fails, feeding RiskManagement.</summary>
public sealed record LoadSecurementFailedEvent(
    LoadSecurementCheckEventId EventId,
    LoadSecurementCheckId LoadSecurementCheckId,
    int DeficiencyCount,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ILoadSecurementCheckDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "LoadSecurementFailed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when corrective action for a failed check is completed.</summary>
public sealed record LoadSecurementCorrectiveActionCompletedEvent(
    LoadSecurementCheckEventId EventId,
    LoadSecurementCheckId LoadSecurementCheckId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ILoadSecurementCheckDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "LoadSecurementCorrectiveActionCompleted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}
