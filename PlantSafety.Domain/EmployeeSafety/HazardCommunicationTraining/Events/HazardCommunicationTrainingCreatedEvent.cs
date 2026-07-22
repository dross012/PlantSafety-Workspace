// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.HazardCommunicationTraining
//  Layer:          Domain
//  File:           HazardCommunicationTrainingCreatedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain events raised across the real HazCom training lifecycle: scheduling,
//                  delivery, comprehension verification, retraining, next-cycle scheduling, and closure.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.HazardCommunicationTraining.Events;

/// <summary>Raised when a new HazCom training cycle is scheduled.</summary>
public sealed record HazardCommunicationTrainingScheduledEvent(
    HazardCommunicationTrainingEventId EventId,
    HazardCommunicationTrainingId HazardCommunicationTrainingId,
    GhsHazardClass HazardClass,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IHazardCommunicationTrainingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "HazardCommunicationTrainingScheduled";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when training is delivered.</summary>
public sealed record HazardCommunicationTrainingDeliveredEvent(
    HazardCommunicationTrainingEventId EventId,
    HazardCommunicationTrainingId HazardCommunicationTrainingId,
    int EmployeesTrainedCount,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IHazardCommunicationTrainingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "HazardCommunicationTrainingDelivered";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when comprehension is verified above the passing threshold.</summary>
public sealed record HazardCommunicationTrainingComprehensionVerifiedEvent(
    HazardCommunicationTrainingEventId EventId,
    HazardCommunicationTrainingId HazardCommunicationTrainingId,
    decimal ComprehensionScore,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IHazardCommunicationTrainingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "HazardCommunicationTrainingComprehensionVerified";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when comprehension falls below the passing threshold and retraining is required.</summary>
public sealed record HazardCommunicationTrainingRetrainingRequiredEvent(
    HazardCommunicationTrainingEventId EventId,
    HazardCommunicationTrainingId HazardCommunicationTrainingId,
    decimal ComprehensionScore,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IHazardCommunicationTrainingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "HazardCommunicationTrainingRetrainingRequired";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the next mandatory training cycle is scheduled.</summary>
public sealed record HazardCommunicationTrainingNextCycleScheduledEvent(
    HazardCommunicationTrainingEventId EventId,
    HazardCommunicationTrainingId HazardCommunicationTrainingId,
    UtcTimestamp DueAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IHazardCommunicationTrainingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "HazardCommunicationTrainingNextCycleScheduled";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => DueAtUtc;
}

/// <summary>Raised when the training cycle record is closed.</summary>
public sealed record HazardCommunicationTrainingClosedEvent(
    HazardCommunicationTrainingEventId EventId,
    HazardCommunicationTrainingId HazardCommunicationTrainingId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IHazardCommunicationTrainingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "HazardCommunicationTrainingClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}
