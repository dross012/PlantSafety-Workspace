// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.LotoSystems
//  Layer:          Domain
//  File:           LotoSystemsCreatedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain events raised across the real machine-level LOTO system lifecycle:
//                  documentation, isolation application, zero-energy verification, release, and audit.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.LotoSystems.Events;

/// <summary>Raised when a new machine-level LOTO system is documented.</summary>
public sealed record LotoSystemDocumentedEvent(
    LotoSystemsEventId EventId,
    LotoSystemsId LotoSystemsId,
    MachineEnergySourceType EnergySourceTypes,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ILotoSystemsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "LotoSystemDocumented";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when isolation devices are applied to all isolation points.</summary>
public sealed record LotoSystemIsolationAppliedEvent(
    LotoSystemsEventId EventId,
    LotoSystemsId LotoSystemsId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ILotoSystemsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "LotoSystemIsolationApplied";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when zero-energy state is verified.</summary>
public sealed record LotoSystemZeroEnergyVerifiedEvent(
    LotoSystemsEventId EventId,
    LotoSystemsId LotoSystemsId,
    UserId VerifiedByUserId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ILotoSystemsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "LotoSystemZeroEnergyVerified";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when isolation is released and the machine returns to service.</summary>
public sealed record LotoSystemReleasedEvent(
    LotoSystemsEventId EventId,
    LotoSystemsId LotoSystemsId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ILotoSystemsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "LotoSystemReleased";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the mandatory annual audit is recorded.</summary>
public sealed record LotoSystemAuditRecordedEvent(
    LotoSystemsEventId EventId,
    LotoSystemsId LotoSystemsId,
    bool Compliant,
    decimal ComplianceScore,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ILotoSystemsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "LotoSystemAuditRecorded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}
