// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.MachineGuarding
//  Layer:          Domain
//  File:           MachineGuardingCreatedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain events raised across the MachineGuarding aggregate's real inspection
//                  lifecycle: installation, inspection scheduling, pass/fail results, bypass detection,
//                  corrective action, and retirement.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.MachineGuarding.Events;

/// <summary>Raised when a new machine guard is installed and recorded.</summary>
public sealed record MachineGuardingInstalledEvent(
    MachineGuardingEventId EventId,
    MachineGuardingId MachineGuardingId,
    MachineGuardType GuardType,
    string EquipmentTag,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IMachineGuardingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "MachineGuardingInstalled";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the next inspection for a guard is scheduled.</summary>
public sealed record MachineGuardingInspectionScheduledEvent(
    MachineGuardingEventId EventId,
    MachineGuardingId MachineGuardingId,
    UtcTimestamp DueAtUtc,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IMachineGuardingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "MachineGuardingInspectionScheduled";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when a scheduled inspection passes with zero open deficiencies.</summary>
public sealed record MachineGuardingInspectionPassedEvent(
    MachineGuardingEventId EventId,
    MachineGuardingId MachineGuardingId,
    decimal GuardConditionScore,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IMachineGuardingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "MachineGuardingInspectionPassed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when a scheduled inspection fails or records open deficiencies.</summary>
public sealed record MachineGuardingInspectionFailedEvent(
    MachineGuardingEventId EventId,
    MachineGuardingId MachineGuardingId,
    int DeficiencyCount,
    string Notes,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IMachineGuardingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "MachineGuardingInspectionFailed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when a guard or interlock is found bypassed, defeated, or removed.</summary>
public sealed record MachineGuardingBypassDetectedEvent(
    MachineGuardingEventId EventId,
    MachineGuardingId MachineGuardingId,
    string FindingNotes,
    UserId DetectedByUserId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IMachineGuardingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "MachineGuardingBypassDetected";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when a corrective-action cycle is opened for a failed inspection or bypass finding.</summary>
public sealed record MachineGuardingCorrectiveActionScheduledEvent(
    MachineGuardingEventId EventId,
    MachineGuardingId MachineGuardingId,
    UtcTimestamp DueAtUtc,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IMachineGuardingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "MachineGuardingCorrectiveActionScheduled";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when corrective action is verified complete and the guard returns to compliant service.</summary>
public sealed record MachineGuardingCorrectiveActionVerifiedEvent(
    MachineGuardingEventId EventId,
    MachineGuardingId MachineGuardingId,
    UserId VerifiedByUserId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IMachineGuardingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "MachineGuardingCorrectiveActionVerified";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when a guard record is retired.</summary>
public sealed record MachineGuardingRetiredEvent(
    MachineGuardingEventId EventId,
    MachineGuardingId MachineGuardingId,
    string Reason,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IMachineGuardingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "MachineGuardingRetired";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}
