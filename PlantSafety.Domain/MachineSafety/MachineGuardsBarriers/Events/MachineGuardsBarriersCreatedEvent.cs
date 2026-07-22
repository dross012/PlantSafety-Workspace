// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.MachineGuardsBarriers
//  Layer:          Domain
//  File:           MachineGuardsBarriersCreatedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain events raised across the real guard/barrier inspection lifecycle:
//                  installation, scheduling, compliance/deficiency outcome, corrective action, and
//                  retirement.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.MachineGuardsBarriers.Events;

/// <summary>Raised when a new guard/barrier is installed.</summary>
public sealed record MachineGuardsBarriersInstalledEvent(
    MachineGuardsBarriersEventId EventId,
    MachineGuardsBarriersId MachineGuardsBarriersId,
    MachineGuardBarrierType GuardType,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IMachineGuardsBarriersDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "MachineGuardsBarriersInstalled";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the next inspection is scheduled.</summary>
public sealed record MachineGuardsBarriersInspectionScheduledEvent(
    MachineGuardsBarriersEventId EventId,
    MachineGuardsBarriersId MachineGuardsBarriersId,
    UtcTimestamp DueAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IMachineGuardsBarriersDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "MachineGuardsBarriersInspectionScheduled";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => DueAtUtc;
}

/// <summary>Raised when an inspection finds the guard compliant.</summary>
public sealed record MachineGuardsBarriersCompliantEvent(
    MachineGuardsBarriersEventId EventId,
    MachineGuardsBarriersId MachineGuardsBarriersId,
    decimal GuardIntegrityScore,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IMachineGuardsBarriersDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "MachineGuardsBarriersCompliant";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when an inspection finds a deficiency.</summary>
public sealed record MachineGuardsBarriersDeficiencyFoundEvent(
    MachineGuardsBarriersEventId EventId,
    MachineGuardsBarriersId MachineGuardsBarriersId,
    int DeficiencyCount,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IMachineGuardsBarriersDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "MachineGuardsBarriersDeficiencyFound";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when corrective action begins to remediate a deficiency.</summary>
public sealed record MachineGuardsBarriersCorrectiveActionStartedEvent(
    MachineGuardsBarriersEventId EventId,
    MachineGuardsBarriersId MachineGuardsBarriersId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IMachineGuardsBarriersDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "MachineGuardsBarriersCorrectiveActionStarted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when corrective action is verified complete.</summary>
public sealed record MachineGuardsBarriersCorrectiveActionVerifiedEvent(
    MachineGuardsBarriersEventId EventId,
    MachineGuardsBarriersId MachineGuardsBarriersId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IMachineGuardsBarriersDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "MachineGuardsBarriersCorrectiveActionVerified";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the guard/barrier is retired.</summary>
public sealed record MachineGuardsBarriersRetiredEvent(
    MachineGuardsBarriersEventId EventId,
    MachineGuardsBarriersId MachineGuardsBarriersId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IMachineGuardsBarriersDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "MachineGuardsBarriersRetired";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}
