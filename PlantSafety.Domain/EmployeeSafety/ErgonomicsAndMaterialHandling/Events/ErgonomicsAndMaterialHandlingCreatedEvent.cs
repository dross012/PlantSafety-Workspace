// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.ErgonomicsAndMaterialHandling
//  Layer:          Domain
//  File:           ErgonomicsAndMaterialHandlingCreatedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain events raised across the real ergonomic assessment lifecycle: scheduling,
//                  assessment, control implementation, follow-up, and closure.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.ErgonomicsAndMaterialHandling.Events;

/// <summary>Raised when a new ergonomic assessment is scheduled.</summary>
public sealed record ErgonomicsAssessmentScheduledEvent(
    ErgonomicsAndMaterialHandlingEventId EventId,
    ErgonomicsAndMaterialHandlingId ErgonomicsAndMaterialHandlingId,
    ErgonomicHazardCategory HazardCategory,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IErgonomicsAndMaterialHandlingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ErgonomicsAssessmentScheduled";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when a completed assessment's risk factors and score are recorded.</summary>
public sealed record ErgonomicsAssessmentRecordedEvent(
    ErgonomicsAndMaterialHandlingEventId EventId,
    ErgonomicsAndMaterialHandlingId ErgonomicsAndMaterialHandlingId,
    int RiskFactorCount,
    decimal RiskScore,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IErgonomicsAndMaterialHandlingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ErgonomicsAssessmentRecorded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when mitigating controls are implemented.</summary>
public sealed record ErgonomicsControlsImplementedEvent(
    ErgonomicsAndMaterialHandlingEventId EventId,
    ErgonomicsAndMaterialHandlingId ErgonomicsAndMaterialHandlingId,
    string ControlsDescription,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IErgonomicsAndMaterialHandlingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ErgonomicsControlsImplemented";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when a follow-up assessment is scheduled.</summary>
public sealed record ErgonomicsFollowUpScheduledEvent(
    ErgonomicsAndMaterialHandlingEventId EventId,
    ErgonomicsAndMaterialHandlingId ErgonomicsAndMaterialHandlingId,
    UtcTimestamp DueAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IErgonomicsAndMaterialHandlingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ErgonomicsFollowUpScheduled";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => DueAtUtc;
}

/// <summary>Raised when the assessment is closed.</summary>
public sealed record ErgonomicsAssessmentClosedEvent(
    ErgonomicsAndMaterialHandlingEventId EventId,
    ErgonomicsAndMaterialHandlingId ErgonomicsAndMaterialHandlingId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IErgonomicsAndMaterialHandlingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ErgonomicsAssessmentClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}
