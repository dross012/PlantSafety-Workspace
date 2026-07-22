// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.HazardCommunicationRightToKnow
//  Layer:          Domain
//  File:           HazardCommunicationRightToKnowCreatedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain events raised across the real HazCom procedure document lifecycle: drafting,
//                  submission, publication, revision, and retirement.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.HazardCommunicationRightToKnow.Events;

/// <summary>Raised when a new HazCom procedure is drafted.</summary>
public sealed record HazComProcedureDraftedEvent(
    HazardCommunicationRightToKnowEventId EventId,
    HazardCommunicationRightToKnowId HazardCommunicationRightToKnowId,
    ProcedureHazardClass HazardClass,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IHazardCommunicationRightToKnowDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "HazComProcedureDrafted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the procedure is submitted for approval.</summary>
public sealed record HazComProcedureSubmittedForApprovalEvent(
    HazardCommunicationRightToKnowEventId EventId,
    HazardCommunicationRightToKnowId HazardCommunicationRightToKnowId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IHazardCommunicationRightToKnowDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "HazComProcedureSubmittedForApproval";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the procedure is approved and published.</summary>
public sealed record HazComProcedurePublishedEvent(
    HazardCommunicationRightToKnowEventId EventId,
    HazardCommunicationRightToKnowId HazardCommunicationRightToKnowId,
    UserId ApprovedByUserId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IHazardCommunicationRightToKnowDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "HazComProcedurePublished";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when a revision cycle is opened.</summary>
public sealed record HazComProcedureRevisionScheduledEvent(
    HazardCommunicationRightToKnowEventId EventId,
    HazardCommunicationRightToKnowId HazardCommunicationRightToKnowId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IHazardCommunicationRightToKnowDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "HazComProcedureRevisionScheduled";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the procedure document is retired.</summary>
public sealed record HazComProcedureRetiredEvent(
    HazardCommunicationRightToKnowEventId EventId,
    HazardCommunicationRightToKnowId HazardCommunicationRightToKnowId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IHazardCommunicationRightToKnowDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "HazComProcedureRetired";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}
