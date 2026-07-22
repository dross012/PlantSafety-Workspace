// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.EmergencyPreparedness
//  Layer:          Domain
//  File:           EmergencyPreparednessCreatedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain events raised across the real emergency preparedness lifecycle: plan
//                  documentation, drill scheduling, drill execution, and readiness recording.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.EmergencyPreparedness.Events;

/// <summary>Raised when a new emergency response plan is documented.</summary>
public sealed record EmergencyPreparednessDocumentedEvent(
    EmergencyPreparednessEventId EventId,
    EmergencyPreparednessId EmergencyPreparednessId,
    EmergencyType EmergencyType,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEmergencyPreparednessDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EmergencyPreparednessDocumented";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the next drill is scheduled.</summary>
public sealed record EmergencyPreparednessDrillScheduledEvent(
    EmergencyPreparednessEventId EventId,
    EmergencyPreparednessId EmergencyPreparednessId,
    UtcTimestamp DueAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEmergencyPreparednessDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EmergencyPreparednessDrillScheduled";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => DueAtUtc;
}

/// <summary>Raised when a scheduled drill is conducted.</summary>
public sealed record EmergencyPreparednessDrillConductedEvent(
    EmergencyPreparednessEventId EventId,
    EmergencyPreparednessId EmergencyPreparednessId,
    int ParticipantCount,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEmergencyPreparednessDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EmergencyPreparednessDrillConducted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when readiness is verified at or above the acceptable threshold.</summary>
public sealed record EmergencyPreparednessReadinessVerifiedEvent(
    EmergencyPreparednessEventId EventId,
    EmergencyPreparednessId EmergencyPreparednessId,
    decimal ReadinessScore,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEmergencyPreparednessDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EmergencyPreparednessReadinessVerified";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when a readiness gap is identified from a drill.</summary>
public sealed record EmergencyPreparednessReadinessGapIdentifiedEvent(
    EmergencyPreparednessEventId EventId,
    EmergencyPreparednessId EmergencyPreparednessId,
    decimal ReadinessScore,
    string GapNotes,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEmergencyPreparednessDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EmergencyPreparednessReadinessGapIdentified";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}
