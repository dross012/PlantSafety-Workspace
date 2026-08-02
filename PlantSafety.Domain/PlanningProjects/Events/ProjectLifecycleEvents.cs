// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PlanningProjects
//  Layer:          Domain
//  File:           ProjectLifecycleEvents.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Domain events for the Project aggregate's full lifecycle (Create/AddMilestone/
//                  AchieveMilestone/Activate/PutOnHold/Resume/Complete/Cancel/Archive). Phase 4
//                  full build-out -- this aggregate did not exist before this pass (only the
//                  ProjectMilestone child entity, ProjectStatus enum, and ProjectId value object
//                  existed).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PlanningProjects.Events;

/// <summary>Raised when a new planning project is created.</summary>
public sealed record ProjectCreatedEvent(
    ProjectEventId EventId,
    ProjectId ProjectId,
    Guid TenantId,
    Guid? SiteId,
    string Name,
    UtcTimestamp TargetCompletionUtc,
    UserId OwnerId,
    UtcTimestamp CreatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPlanningProjectsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ProjectCreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CreatedAtUtc;
}

/// <summary>Raised when a milestone is added to a project.</summary>
public sealed record ProjectMilestoneAddedEvent(
    ProjectEventId EventId,
    ProjectId ProjectId,
    Guid MilestoneId,
    string Name,
    UtcTimestamp TargetDateUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPlanningProjectsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ProjectMilestoneAdded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => TargetDateUtc;
}

/// <summary>Raised when a project milestone is achieved.</summary>
public sealed record ProjectMilestoneAchievedEvent(
    ProjectEventId EventId,
    ProjectId ProjectId,
    Guid MilestoneId,
    UtcTimestamp AchievedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPlanningProjectsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ProjectMilestoneAchieved";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => AchievedAtUtc;
}

/// <summary>Raised when a project is activated (Planning -&gt; Active).</summary>
public sealed record ProjectActivatedEvent(
    ProjectEventId EventId,
    ProjectId ProjectId,
    UserId ActivatedBy,
    UtcTimestamp ActivatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPlanningProjectsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ProjectActivated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ActivatedAtUtc;
}

/// <summary>Raised when a project is put on hold (Active -&gt; OnHold).</summary>
public sealed record ProjectPutOnHoldEvent(
    ProjectEventId EventId,
    ProjectId ProjectId,
    UserId PutOnHoldBy,
    string? Reason,
    UtcTimestamp PutOnHoldAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPlanningProjectsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ProjectPutOnHold";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => PutOnHoldAtUtc;
}

/// <summary>Raised when a project resumes from hold (OnHold -&gt; Active).</summary>
public sealed record ProjectResumedEvent(
    ProjectEventId EventId,
    ProjectId ProjectId,
    UserId ResumedBy,
    UtcTimestamp ResumedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPlanningProjectsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ProjectResumed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ResumedAtUtc;
}

/// <summary>Raised when a project is completed (Active -&gt; Completed).</summary>
public sealed record ProjectCompletedEvent(
    ProjectEventId EventId,
    ProjectId ProjectId,
    UserId CompletedBy,
    UtcTimestamp CompletedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPlanningProjectsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ProjectCompleted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CompletedAtUtc;
}

/// <summary>Raised when a project is cancelled (Planning/Active/OnHold -&gt; Cancelled).</summary>
public sealed record ProjectCancelledEvent(
    ProjectEventId EventId,
    ProjectId ProjectId,
    UserId CancelledBy,
    string? Reason,
    UtcTimestamp CancelledAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPlanningProjectsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ProjectCancelled";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CancelledAtUtc;
}

/// <summary>Raised when a project is archived (Completed/Cancelled -&gt; Archived).</summary>
public sealed record ProjectArchivedEvent(
    ProjectEventId EventId,
    ProjectId ProjectId,
    UtcTimestamp ArchivedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPlanningProjectsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ProjectArchived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ArchivedAtUtc;
}
