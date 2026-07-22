// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.Ppe
//  Layer:          Domain
//  File:           PpeCreatedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain events raised across the Ppe real issuance lifecycle: issuance, fit testing,
//                  inspection, replacement flagging, and retirement.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.Ppe.Events;

/// <summary>Raised when a new PPE item is issued to an employee.</summary>
public sealed record PpeIssuedEvent(
    PpeEventId EventId,
    PpeId PpeId,
    PpeCategory Category,
    UserId IssuedToUserId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPpeDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PpeIssued";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when a required fit test is recorded.</summary>
public sealed record PpeFitTestRecordedEvent(
    PpeEventId EventId,
    PpeId PpeId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPpeDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PpeFitTestRecorded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when a periodic condition inspection is recorded.</summary>
public sealed record PpeInspectionRecordedEvent(
    PpeEventId EventId,
    PpeId PpeId,
    decimal ConditionScore,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPpeDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PpeInspectionRecorded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when an item is flagged for replacement.</summary>
public sealed record PpeFlaggedForReplacementEvent(
    PpeEventId EventId,
    PpeId PpeId,
    string Reason,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPpeDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PpeFlaggedForReplacement";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when an item is retired from service.</summary>
public sealed record PpeRetiredEvent(
    PpeEventId EventId,
    PpeId PpeId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPpeDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PpeRetired";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}
