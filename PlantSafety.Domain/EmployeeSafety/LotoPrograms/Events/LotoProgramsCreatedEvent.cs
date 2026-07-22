// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.LotoPrograms
//  Layer:          Domain
//  File:           LotoProgramsCreatedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain events raised across the LotoPrograms real lifecycle: documentation,
//                  isolation application, zero-energy verification, release, and annual audit.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.LotoPrograms.Events;

/// <summary>Raised when a new LOTO program is documented.</summary>
public sealed record LotoProgramsDocumentedEvent(
    LotoProgramsEventId EventId,
    LotoProgramsId LotoProgramsId,
    LotoEnergySourceType EnergySourceTypes,
    int IsolationPointCount,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ILotoProgramsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "LotoProgramsDocumented";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when locks/hasps/tags are applied to all isolation points.</summary>
public sealed record LotoProgramsIsolationAppliedEvent(
    LotoProgramsEventId EventId,
    LotoProgramsId LotoProgramsId,
    int LocksAppliedCount,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ILotoProgramsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "LotoProgramsIsolationApplied";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when zero-energy state is verified by an authorized employee.</summary>
public sealed record LotoProgramsZeroEnergyVerifiedEvent(
    LotoProgramsEventId EventId,
    LotoProgramsId LotoProgramsId,
    UserId VerifiedByUserId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ILotoProgramsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "LotoProgramsZeroEnergyVerified";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when isolation devices are removed and the equipment is released back to service.</summary>
public sealed record LotoProgramsReleasedEvent(
    LotoProgramsEventId EventId,
    LotoProgramsId LotoProgramsId,
    UserId ReleasedByUserId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ILotoProgramsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "LotoProgramsReleased";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the mandatory annual LOTO program audit is recorded.</summary>
public sealed record LotoProgramsAuditRecordedEvent(
    LotoProgramsEventId EventId,
    LotoProgramsId LotoProgramsId,
    bool Compliant,
    decimal ComplianceScore,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ILotoProgramsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "LotoProgramsAuditRecorded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}
