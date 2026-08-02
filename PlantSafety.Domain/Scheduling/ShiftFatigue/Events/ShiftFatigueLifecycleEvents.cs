// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.ShiftFatigue
//  Layer:          Domain
//  File:           ShiftFatigueLifecycleEvents.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Domain events for the ShiftFatigue aggregate's lifecycle (Activate/
//                  ReassessRisk/Complete/Close/Archive), raised alongside the pre-existing
//                  ShiftFatigueCreatedEvent. Phase 4 full build-out -- these transitions
//                  previously did not exist on the aggregate (only a Create factory did, though
//                  ShiftFatigueLifecycleRules.Can* guards and the
//                  Created/Active/Completed/Closed/Archived status enum were already scaffolded
//                  and unused).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.ShiftFatigue.Events;

/// <summary>Raised when fatigue monitoring for the employee's shift record is activated (Created -&gt; Active).</summary>
public sealed record ShiftFatigueActivatedEvent(
    ShiftFatigueEventId EventId,
    ShiftFatigueId ShiftFatigueId,
    UserId ActivatedBy,
    UtcTimestamp ActivatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IShiftFatigueDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ShiftFatigueActivated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ActivatedAtUtc;
}

/// <summary>Raised when the fatigue risk score is reassessed during active monitoring. Does not change lifecycle status.</summary>
public sealed record ShiftFatigueRiskReassessedEvent(
    ShiftFatigueEventId EventId,
    ShiftFatigueId ShiftFatigueId,
    UserId ReassessedBy,
    decimal FatigueRiskScore,
    int ConsecutiveNightShifts,
    decimal OvertimeHours,
    UtcTimestamp ReassessedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IShiftFatigueDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ShiftFatigueRiskReassessed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ReassessedAtUtc;
}

/// <summary>Raised when the monitoring period is completed (Active -&gt; Completed).</summary>
public sealed record ShiftFatigueCompletedEvent(
    ShiftFatigueEventId EventId,
    ShiftFatigueId ShiftFatigueId,
    UserId CompletedBy,
    UtcTimestamp CompletedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IShiftFatigueDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ShiftFatigueCompleted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CompletedAtUtc;
}

/// <summary>Raised when a completed fatigue record is closed (Completed -&gt; Closed).</summary>
public sealed record ShiftFatigueClosedEvent(
    ShiftFatigueEventId EventId,
    ShiftFatigueId ShiftFatigueId,
    UserId ClosedBy,
    UtcTimestamp ClosedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IShiftFatigueDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ShiftFatigueClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ClosedAtUtc;
}

/// <summary>Raised when a closed fatigue record is archived for compliance retention (Closed -&gt; Archived).</summary>
public sealed record ShiftFatigueArchivedEvent(
    ShiftFatigueEventId EventId,
    ShiftFatigueId ShiftFatigueId,
    UtcTimestamp ArchivedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IShiftFatigueDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ShiftFatigueArchived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ArchivedAtUtc;
}
