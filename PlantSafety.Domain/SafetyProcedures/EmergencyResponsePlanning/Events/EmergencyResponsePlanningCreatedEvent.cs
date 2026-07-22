// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.EmergencyResponsePlanning
//  Layer:          Domain
//  File:           EmergencyResponsePlanningCreatedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain events raised across the real emergency response plan lifecycle: drafting,
//                  submission, publication, drill scheduling/recording, and retirement.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.EmergencyResponsePlanning.Events;

/// <summary>Raised when a new emergency response plan is drafted.</summary>
public sealed record EmergencyPlanDraftedEvent(
    EmergencyResponsePlanningEventId EventId,
    EmergencyResponsePlanningId EmergencyResponsePlanningId,
    ProcedureEmergencyType EmergencyType,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEmergencyResponsePlanningDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EmergencyPlanDrafted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the plan is submitted for approval.</summary>
public sealed record EmergencyPlanSubmittedForApprovalEvent(
    EmergencyResponsePlanningEventId EventId,
    EmergencyResponsePlanningId EmergencyResponsePlanningId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEmergencyResponsePlanningDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EmergencyPlanSubmittedForApproval";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the plan is approved and published.</summary>
public sealed record EmergencyPlanPublishedEvent(
    EmergencyResponsePlanningEventId EventId,
    EmergencyResponsePlanningId EmergencyResponsePlanningId,
    UserId ApprovedByUserId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEmergencyResponsePlanningDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EmergencyPlanPublished";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the next drill is scheduled.</summary>
public sealed record EmergencyPlanDrillScheduledEvent(
    EmergencyResponsePlanningEventId EventId,
    EmergencyResponsePlanningId EmergencyResponsePlanningId,
    UtcTimestamp DueAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEmergencyResponsePlanningDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EmergencyPlanDrillScheduled";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => DueAtUtc;
}

/// <summary>Raised when a conducted drill's outcome is recorded.</summary>
public sealed record EmergencyPlanDrillRecordedEvent(
    EmergencyResponsePlanningEventId EventId,
    EmergencyResponsePlanningId EmergencyResponsePlanningId,
    int ParticipantCount,
    decimal ReadinessScore,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEmergencyResponsePlanningDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EmergencyPlanDrillRecorded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the plan document is retired.</summary>
public sealed record EmergencyPlanRetiredEvent(
    EmergencyResponsePlanningEventId EventId,
    EmergencyResponsePlanningId EmergencyResponsePlanningId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEmergencyResponsePlanningDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EmergencyPlanRetired";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}
