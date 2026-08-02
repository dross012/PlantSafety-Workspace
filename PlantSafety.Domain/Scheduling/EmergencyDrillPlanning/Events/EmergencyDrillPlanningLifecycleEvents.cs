// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.EmergencyDrillPlanning
//  Layer:          Domain
//  File:           EmergencyDrillPlanningLifecycleEvents.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Domain events for the EmergencyDrillPlanning aggregate's lifecycle
//                  (Conduct/Complete/Close/Archive), raised alongside the pre-existing
//                  EmergencyDrillPlanningCreatedEvent. Phase 4 full build-out -- these
//                  transitions previously did not exist on the aggregate (only a Create factory
//                  did, though EmergencyDrillPlanningLifecycleRules.Can* guards and the
//                  Created/Active/Completed/Closed/Archived status enum were already scaffolded
//                  and unused).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Events;

/// <summary>Raised when a scheduled drill is conducted (Created -&gt; Active).</summary>
public sealed record EmergencyDrillPlanningConductedEvent(
    EmergencyDrillPlanningEventId EventId,
    EmergencyDrillPlanningId EmergencyDrillPlanningId,
    UserId ConductedBy,
    int ParticipationCount,
    UtcTimestamp ConductedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEmergencyDrillPlanningDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EmergencyDrillPlanningConducted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ConductedAtUtc;
}

/// <summary>Raised when the post-drill debrief/completion is recorded (Active -&gt; Completed).</summary>
public sealed record EmergencyDrillPlanningCompletedEvent(
    EmergencyDrillPlanningEventId EventId,
    EmergencyDrillPlanningId EmergencyDrillPlanningId,
    UserId CompletedBy,
    string? DebriefNotes,
    UtcTimestamp CompletedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEmergencyDrillPlanningDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EmergencyDrillPlanningCompleted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CompletedAtUtc;
}

/// <summary>Raised when a completed drill record is closed (Completed -&gt; Closed).</summary>
public sealed record EmergencyDrillPlanningClosedEvent(
    EmergencyDrillPlanningEventId EventId,
    EmergencyDrillPlanningId EmergencyDrillPlanningId,
    UserId ClosedBy,
    UtcTimestamp ClosedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEmergencyDrillPlanningDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EmergencyDrillPlanningClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ClosedAtUtc;
}

/// <summary>Raised when a closed drill record is archived for compliance retention (Closed -&gt; Archived).</summary>
public sealed record EmergencyDrillPlanningArchivedEvent(
    EmergencyDrillPlanningEventId EventId,
    EmergencyDrillPlanningId EmergencyDrillPlanningId,
    UtcTimestamp ArchivedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IEmergencyDrillPlanningDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EmergencyDrillPlanningArchived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ArchivedAtUtc;
}
