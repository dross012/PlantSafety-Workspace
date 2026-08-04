// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.CraneHoistSystems
//  Layer:          Domain
//  File:           CraneHoistSystemsCreatedEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain events raised across the crane/hoist asset lifecycle: registration,
//                  lift-event recording, overload detection, inspection scheduling/completion,
//                  operator qualification, and rigging inspection.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.CraneHoistSystems.Events;

/// <summary>Raised when a new crane/hoist asset is registered in the fleet.</summary>
public sealed record CraneHoistAssetRegisteredEvent(
    CraneHoistSystemsEventId EventId,
    CraneHoistSystemsId CraneHoistSystemsId,
    CraneHoistAssetType AssetType,
    decimal RatedLoadCapacityKg,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ICraneHoistSystemsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CraneHoistAssetRegistered";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when a lift event is recorded against a crane/hoist asset.</summary>
public sealed record LiftEventRecordedEvent(
    CraneHoistSystemsEventId EventId,
    CraneHoistSystemsId CraneHoistSystemsId,
    decimal ObservedLoadKg,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ICraneHoistSystemsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "LiftEventRecorded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when an observed load exceeds the asset's rated load capacity.</summary>
public sealed record LoadLimitExceededEvent(
    CraneHoistSystemsEventId EventId,
    CraneHoistSystemsId CraneHoistSystemsId,
    decimal ObservedLoadKg,
    decimal RatedLoadCapacityKg,
    int OverloadEventCount,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ICraneHoistSystemsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "LoadLimitExceeded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when a crane inspection is scheduled.</summary>
public sealed record CraneInspectionScheduledEvent(
    CraneHoistSystemsEventId EventId,
    CraneHoistSystemsId CraneHoistSystemsId,
    UtcTimestamp DueAtUtc,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ICraneHoistSystemsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CraneInspectionScheduled";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when a scheduled crane inspection is completed.</summary>
public sealed record CraneInspectionCompletedEvent(
    CraneHoistSystemsEventId EventId,
    CraneHoistSystemsId CraneHoistSystemsId,
    bool Passed,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ICraneHoistSystemsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CraneInspectionCompleted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when a crane inspection is completed after its scheduled due date, or is
/// otherwise detected to be overdue.</summary>
public sealed record CraneInspectionOverdueEvent(
    CraneHoistSystemsEventId EventId,
    CraneHoistSystemsId CraneHoistSystemsId,
    UtcTimestamp DueAtUtc,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ICraneHoistSystemsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CraneInspectionOverdue";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when an operator is qualified to operate a crane/hoist asset.</summary>
public sealed record OperatorQualifiedEvent(
    CraneHoistSystemsEventId EventId,
    CraneHoistSystemsId CraneHoistSystemsId,
    UserId OperatorUserId,
    UtcTimestamp? ExpiresAtUtc,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ICraneHoistSystemsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "OperatorQualified";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when an operator's qualification to operate a crane/hoist asset is suspended.</summary>
public sealed record OperatorQualificationSuspendedEvent(
    CraneHoistSystemsEventId EventId,
    CraneHoistSystemsId CraneHoistSystemsId,
    UserId OperatorUserId,
    string Reason,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ICraneHoistSystemsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "OperatorQualificationSuspended";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when a rigging inspection detects a defect.</summary>
public sealed record RiggingDefectDetectedEvent(
    CraneHoistSystemsEventId EventId,
    CraneHoistSystemsId CraneHoistSystemsId,
    string Notes,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ICraneHoistSystemsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "RiggingDefectDetected";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}
