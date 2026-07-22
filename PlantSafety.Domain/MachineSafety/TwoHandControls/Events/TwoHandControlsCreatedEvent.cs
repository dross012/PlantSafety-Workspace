// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.TwoHandControls
//  Layer:          Domain
//  File:           TwoHandControlsCreatedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain events raised across the real two-hand control lifecycle: installation, test
//                  scheduling, synchronized/bypass outcome, and retirement.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.TwoHandControls.Events;

/// <summary>Raised when a new two-hand control is installed.</summary>
public sealed record TwoHandControlInstalledEvent(
    TwoHandControlsEventId EventId,
    TwoHandControlsId TwoHandControlsId,
    TwoHandControlEquipmentType EquipmentType,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ITwoHandControlsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "TwoHandControlInstalled";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the next synchronization test is scheduled.</summary>
public sealed record TwoHandControlTestScheduledEvent(
    TwoHandControlsEventId EventId,
    TwoHandControlsId TwoHandControlsId,
    UtcTimestamp DueAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ITwoHandControlsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "TwoHandControlTestScheduled";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => DueAtUtc;
}

/// <summary>Raised when a test confirms proper synchronization.</summary>
public sealed record TwoHandControlSynchronizedEvent(
    TwoHandControlsEventId EventId,
    TwoHandControlsId TwoHandControlsId,
    decimal SynchronizationScore,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ITwoHandControlsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "TwoHandControlSynchronized";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when a bypass attempt or synchronization failure is detected.</summary>
public sealed record TwoHandControlBypassDetectedEvent(
    TwoHandControlsEventId EventId,
    TwoHandControlsId TwoHandControlsId,
    int BypassAttemptCount,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ITwoHandControlsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "TwoHandControlBypassDetected";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the control is retired.</summary>
public sealed record TwoHandControlRetiredEvent(
    TwoHandControlsEventId EventId,
    TwoHandControlsId TwoHandControlsId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ITwoHandControlsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "TwoHandControlRetired";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}
