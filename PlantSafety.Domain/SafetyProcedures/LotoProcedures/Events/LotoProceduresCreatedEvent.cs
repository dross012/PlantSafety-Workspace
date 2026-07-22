// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.LotoProcedures
//  Layer:          Domain
//  File:           LotoProceduresCreatedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain events raised across the real LOTO procedure document lifecycle: drafting,
//                  submission, publication, annual audit pass/flag, revision, and retirement.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.LotoProcedures.Events;

/// <summary>Raised when a new LOTO procedure is drafted.</summary>
public sealed record LotoProcedureDraftedEvent(
    LotoProceduresEventId EventId,
    LotoProceduresId LotoProceduresId,
    ProcedureEnergySourceType EnergySourceTypes,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ILotoProceduresDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "LotoProcedureDrafted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the procedure is submitted for approval.</summary>
public sealed record LotoProcedureSubmittedForApprovalEvent(
    LotoProceduresEventId EventId,
    LotoProceduresId LotoProceduresId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ILotoProceduresDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "LotoProcedureSubmittedForApproval";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the procedure is approved and published.</summary>
public sealed record LotoProcedurePublishedEvent(
    LotoProceduresEventId EventId,
    LotoProceduresId LotoProceduresId,
    UserId ApprovedByUserId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ILotoProceduresDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "LotoProcedurePublished";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the mandatory annual audit passes.</summary>
public sealed record LotoProcedureAuditPassedEvent(
    LotoProceduresEventId EventId,
    LotoProceduresId LotoProceduresId,
    decimal ComplianceScore,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ILotoProceduresDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "LotoProcedureAuditPassed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the mandatory annual audit flags non-compliance.</summary>
public sealed record LotoProcedureAuditFlaggedEvent(
    LotoProceduresEventId EventId,
    LotoProceduresId LotoProceduresId,
    decimal ComplianceScore,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ILotoProceduresDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "LotoProcedureAuditFlagged";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when a revision cycle is opened.</summary>
public sealed record LotoProcedureRevisionScheduledEvent(
    LotoProceduresEventId EventId,
    LotoProceduresId LotoProceduresId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ILotoProceduresDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "LotoProcedureRevisionScheduled";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the procedure document is retired.</summary>
public sealed record LotoProcedureRetiredEvent(
    LotoProceduresEventId EventId,
    LotoProceduresId LotoProceduresId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ILotoProceduresDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "LotoProcedureRetired";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}
