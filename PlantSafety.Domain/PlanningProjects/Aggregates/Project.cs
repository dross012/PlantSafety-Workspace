// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PlanningProjects
//  Layer:          Domain
//  File:           Project.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the PlanningProjects bounded context. Tracks a single
//                  strategic/capital planning project from its Planning stage through Activation,
//                  optional OnHold/Resume cycles, Completion or Cancellation, and finally Archival.
//                  Phase 4 full build-out: this bounded context previously had no aggregate at all
//                  -- only the child entity ProjectMilestone, the ProjectStatus enum, and the
//                  ProjectId value object existed. Owns a private collection of the pre-existing
//                  ProjectMilestone child entity (reused as-is, including its Achieve() method).
//                  Mirrors PlantSafety.Domain.SafetyEvents.RootCauseAnalysis.Aggregates
//                  .RootCauseAnalysis's guarded-transition/domain-event/exception conventions.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.PlanningProjects.Entities;
using PlantSafety.Domain.PlanningProjects.Enums;
using PlantSafety.Domain.PlanningProjects.Events;
using PlantSafety.Domain.PlanningProjects.Exceptions;
using PlantSafety.Domain.PlanningProjects.Rules;
using PlantSafety.Domain.PlanningProjects.ValueObjects;

namespace PlantSafety.Domain.PlanningProjects.Aggregates;

/// <summary>Aggregate root for the PlanningProjects bounded context.</summary>
public sealed class Project
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this Project aggregate.</summary>
    public ProjectId ProjectId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Project name.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Current lifecycle status.</summary>
    public ProjectStatus Status { get; private set; }

    /// <summary>UTC timestamp when the project is targeted to complete.</summary>
    public UtcTimestamp TargetCompletionUtc { get; private set; }

    /// <summary>Identity of the project owner.</summary>
    public UserId OwnerId { get; private set; }

    // ---- Lifecycle attributes ---------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user/system that created this record.</summary>
    public UserId CreatedBy { get; private set; }

    /// <summary>Identity of the user who activated the project, if activated.</summary>
    public UserId? ActivatedBy { get; private set; }

    /// <summary>UTC instant the project was activated.</summary>
    public UtcTimestamp? ActivatedAtUtc { get; private set; }

    /// <summary>Identity of the user who put the project on hold, if applicable.</summary>
    public UserId? PutOnHoldBy { get; private set; }

    /// <summary>UTC instant the project was put on hold.</summary>
    public UtcTimestamp? PutOnHoldAtUtc { get; private set; }

    /// <summary>Reason the project was put on hold, if supplied.</summary>
    public string? OnHoldReason { get; private set; }

    /// <summary>Identity of the user who completed the project, if completed.</summary>
    public UserId? CompletedBy { get; private set; }

    /// <summary>UTC instant the project was completed.</summary>
    public UtcTimestamp? CompletedAtUtc { get; private set; }

    /// <summary>Identity of the user who cancelled the project, if cancelled.</summary>
    public UserId? CancelledBy { get; private set; }

    /// <summary>UTC instant the project was cancelled.</summary>
    public UtcTimestamp? CancelledAtUtc { get; private set; }

    /// <summary>Reason the project was cancelled, if supplied.</summary>
    public string? CancellationReason { get; private set; }

    /// <summary>UTC instant the project was archived for retention.</summary>
    public UtcTimestamp? ArchivedAtUtc { get; private set; }

    // ---- Milestones (child entity collection) ---------------------------------

    private readonly List<ProjectMilestone> _milestones = [];

    /// <summary>Milestones belonging to this project.</summary>
    public IReadOnlyCollection<ProjectMilestone> Milestones => new ReadOnlyCollection<ProjectMilestone>(_milestones);

    // ---- Domain Events ---------------------------------------------------------

    private readonly List<IPlanningProjectsDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IPlanningProjectsDomainEvent> DomainEvents
        => new ReadOnlyCollection<IPlanningProjectsDomainEvent>(_domainEvents);

    // For rehydration only.
    private Project() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new Project and records <see cref="ProjectCreatedEvent"/>.</summary>
    public static Project Create(
        Guid tenantId,
        Guid? siteId,
        string name,
        UtcTimestamp targetCompletionUtc,
        UserId ownerId,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name must be supplied.", nameof(name));

        var agg = new Project
        {
            ProjectId = ProjectId.New(),
            TenantId = tenantId,
            SiteId = siteId,
            Name = name.Trim(),
            Status = ProjectStatus.Planning,
            TargetCompletionUtc = targetCompletionUtc,
            OwnerId = ownerId,
            CreatedAtUtc = createdAtUtc,
            CreatedBy = ownerId,
        };

        agg.AddDomainEvent(new ProjectCreatedEvent(
            EventId: ProjectEventId.New(),
            ProjectId: agg.ProjectId,
            TenantId: tenantId,
            SiteId: siteId,
            Name: agg.Name,
            TargetCompletionUtc: targetCompletionUtc,
            OwnerId: ownerId,
            CreatedAtUtc: createdAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));

        return agg;
    }

    // ---- Milestones -----------------------------------------------------------

    /// <summary>
    /// Adds a new milestone to the project. Valid only while <see cref="ProjectStatus.Planning"/>
    /// or <see cref="ProjectStatus.Active"/>.
    /// </summary>
    public void AddMilestone(
        string name,
        UtcTimestamp targetDateUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusAllows(correlationId, causationId, ProjectLifecycleRules.CanAddMilestone, Status);

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Milestone name must be supplied.", nameof(name));

        var milestone = new ProjectMilestone(Guid.NewGuid(), name.Trim(), targetDateUtc);
        _milestones.Add(milestone);

        AddDomainEvent(new ProjectMilestoneAddedEvent(
            EventId: ProjectEventId.New(),
            ProjectId: ProjectId,
            MilestoneId: milestone.MilestoneId,
            Name: milestone.Name,
            TargetDateUtc: targetDateUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Marks an existing milestone as achieved. Valid only while <see cref="ProjectStatus.Active"/>.
    /// </summary>
    public void AchieveMilestone(
        Guid milestoneId,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusAllows(correlationId, causationId, ProjectLifecycleRules.CanAchieveMilestone, Status);

        var milestone = _milestones.FirstOrDefault(m => m.MilestoneId == milestoneId)
            ?? throw new ProjectMilestoneNotFoundException(ProjectId, milestoneId, correlationId, causationId);

        milestone.Achieve();

        AddDomainEvent(new ProjectMilestoneAchievedEvent(
            EventId: ProjectEventId.New(),
            ProjectId: ProjectId,
            MilestoneId: milestoneId,
            AchievedAtUtc: UtcTimestamp.Now(),
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Activate -----------------------------------------------------

    /// <summary>Activates the project (Planning -&gt; Active). Valid only from <see cref="ProjectStatus.Planning"/>.</summary>
    public void Activate(
        UserId activatedBy,
        UtcTimestamp activatedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, ProjectLifecycleRules.CanActivate, ProjectStatus.Active);

        ActivatedBy = activatedBy;
        ActivatedAtUtc = activatedAtUtc;
        Status = ProjectStatus.Active;

        AddDomainEvent(new ProjectActivatedEvent(
            EventId: ProjectEventId.New(),
            ProjectId: ProjectId,
            ActivatedBy: activatedBy,
            ActivatedAtUtc: activatedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: PutOnHold -----------------------------------------------------

    /// <summary>Puts the project on hold (Active -&gt; OnHold). Valid only from <see cref="ProjectStatus.Active"/>.</summary>
    public void PutOnHold(
        UserId putOnHoldBy,
        string? reason,
        UtcTimestamp atUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, ProjectLifecycleRules.CanPutOnHold, ProjectStatus.OnHold);

        PutOnHoldBy = putOnHoldBy;
        PutOnHoldAtUtc = atUtc;
        OnHoldReason = reason;
        Status = ProjectStatus.OnHold;

        AddDomainEvent(new ProjectPutOnHoldEvent(
            EventId: ProjectEventId.New(),
            ProjectId: ProjectId,
            PutOnHoldBy: putOnHoldBy,
            Reason: reason,
            PutOnHoldAtUtc: atUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Resume -----------------------------------------------------

    /// <summary>Resumes the project from hold (OnHold -&gt; Active). Valid only from <see cref="ProjectStatus.OnHold"/>.</summary>
    public void Resume(
        UserId resumedBy,
        UtcTimestamp resumedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, ProjectLifecycleRules.CanResume, ProjectStatus.Active);

        Status = ProjectStatus.Active;

        AddDomainEvent(new ProjectResumedEvent(
            EventId: ProjectEventId.New(),
            ProjectId: ProjectId,
            ResumedBy: resumedBy,
            ResumedAtUtc: resumedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Complete -----------------------------------------------------

    /// <summary>Completes the project (Active -&gt; Completed). Valid only from <see cref="ProjectStatus.Active"/>.</summary>
    public void Complete(
        UserId completedBy,
        UtcTimestamp completedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, ProjectLifecycleRules.CanComplete, ProjectStatus.Completed);

        CompletedBy = completedBy;
        CompletedAtUtc = completedAtUtc;
        Status = ProjectStatus.Completed;

        AddDomainEvent(new ProjectCompletedEvent(
            EventId: ProjectEventId.New(),
            ProjectId: ProjectId,
            CompletedBy: completedBy,
            CompletedAtUtc: completedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Cancel -----------------------------------------------------

    /// <summary>
    /// Cancels the project (Planning/Active/OnHold -&gt; Cancelled). Valid from
    /// <see cref="ProjectStatus.Planning"/>, <see cref="ProjectStatus.Active"/>, or
    /// <see cref="ProjectStatus.OnHold"/>.
    /// </summary>
    public void Cancel(
        UserId cancelledBy,
        string? reason,
        UtcTimestamp cancelledAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, ProjectLifecycleRules.CanCancel, ProjectStatus.Cancelled);

        CancelledBy = cancelledBy;
        CancellationReason = reason;
        CancelledAtUtc = cancelledAtUtc;
        Status = ProjectStatus.Cancelled;

        AddDomainEvent(new ProjectCancelledEvent(
            EventId: ProjectEventId.New(),
            ProjectId: ProjectId,
            CancelledBy: cancelledBy,
            Reason: reason,
            CancelledAtUtc: cancelledAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Archive -----------------------------------------------------

    /// <summary>
    /// Archives the project for retention (Completed/Cancelled -&gt; Archived). Valid only from
    /// <see cref="ProjectStatus.Completed"/> or <see cref="ProjectStatus.Cancelled"/>.
    /// </summary>
    public void Archive(
        UtcTimestamp archivedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, ProjectLifecycleRules.CanArchive, ProjectStatus.Archived);

        ArchivedAtUtc = archivedAtUtc;
        Status = ProjectStatus.Archived;

        AddDomainEvent(new ProjectArchivedEvent(
            EventId: ProjectEventId.New(),
            ProjectId: ProjectId,
            ArchivedAtUtc: archivedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IPlanningProjectsDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, Func<ProjectStatus, bool> guard, ProjectStatus expected)
    {
        if (guard(Status)) return;

        throw new ProjectInvalidStatusTransitionException(
            ProjectId, Status.ToString(), expected.ToString(), correlationId, causationId);
    }

    private void EnsureStatusAllows(CorrelationId correlationId, CausationId causationId, Func<ProjectStatus, bool> guard, ProjectStatus current)
    {
        if (guard(current)) return;

        throw new ProjectInvalidStatusTransitionException(
            ProjectId, current.ToString(), current.ToString(), correlationId, causationId);
    }
}
