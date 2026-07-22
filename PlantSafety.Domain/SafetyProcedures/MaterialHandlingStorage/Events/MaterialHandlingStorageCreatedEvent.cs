// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.MaterialHandlingStorage
//  Layer:          Domain
//  File:           MaterialHandlingStorageCreatedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain events raised across the real material handling/storage procedure lifecycle:
//                  drafting, submission, publication, obstruction flagging/resolution, and retirement.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.MaterialHandlingStorage.Events;

/// <summary>Raised when a new material handling/storage procedure is drafted.</summary>
public sealed record MaterialHandlingProcedureDraftedEvent(
    MaterialHandlingStorageEventId EventId,
    MaterialHandlingStorageId MaterialHandlingStorageId,
    MaterialHandlingCategory HandlingCategory,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IMaterialHandlingStorageDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "MaterialHandlingProcedureDrafted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the procedure is submitted for approval.</summary>
public sealed record MaterialHandlingProcedureSubmittedForApprovalEvent(
    MaterialHandlingStorageEventId EventId,
    MaterialHandlingStorageId MaterialHandlingStorageId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IMaterialHandlingStorageDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "MaterialHandlingProcedureSubmittedForApproval";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the procedure is approved and published.</summary>
public sealed record MaterialHandlingProcedurePublishedEvent(
    MaterialHandlingStorageEventId EventId,
    MaterialHandlingStorageId MaterialHandlingStorageId,
    UserId ApprovedByUserId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IMaterialHandlingStorageDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "MaterialHandlingProcedurePublished";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when a walkway/egress obstruction or unsafe storage condition is flagged.</summary>
public sealed record MaterialHandlingObstructionFlaggedEvent(
    MaterialHandlingStorageEventId EventId,
    MaterialHandlingStorageId MaterialHandlingStorageId,
    int ObstructionCount,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IMaterialHandlingStorageDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "MaterialHandlingObstructionFlagged";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when a flagged obstruction is resolved.</summary>
public sealed record MaterialHandlingObstructionResolvedEvent(
    MaterialHandlingStorageEventId EventId,
    MaterialHandlingStorageId MaterialHandlingStorageId,
    decimal ComplianceScore,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IMaterialHandlingStorageDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "MaterialHandlingObstructionResolved";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the procedure document is retired.</summary>
public sealed record MaterialHandlingProcedureRetiredEvent(
    MaterialHandlingStorageEventId EventId,
    MaterialHandlingStorageId MaterialHandlingStorageId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IMaterialHandlingStorageDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "MaterialHandlingProcedureRetired";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}
