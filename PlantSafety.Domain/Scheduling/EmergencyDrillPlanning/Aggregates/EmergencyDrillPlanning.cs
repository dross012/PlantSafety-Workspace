// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.EmergencyDrillPlanning
//  Layer:          Domain
//  File:           EmergencyDrillPlanning.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the EmergencyDrillPlanning bounded context (module: Scheduling).
//                  Additive-only evolution.
//  UPDATED:        2026-07-30 -- Phase 4 full build-out: added Conduct/Complete/Close/Archive
//                  lifecycle methods, wiring the pre-existing (previously unused)
//                  EmergencyDrillPlanningLifecycleRules.Can* guards. Mirrors
//                  PlantSafety.Domain.Compliance.Permits.Aggregates.Permits's conventions.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Aggregates;

/// <summary>Aggregate root for the EmergencyDrillPlanning bounded context.</summary>
public sealed partial class EmergencyDrillPlanning
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this EmergencyDrillPlanning aggregate.</summary>
    public EmergencyDrillPlanningId EmergencyDrillPlanningId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public EmergencyDrillPlanningStatus Status { get; private set; }

    // ---- EmergencyDrillPlanning-specific business properties --------------------------------

    /// <summary>DrillType business attribute.</summary>
    public string DrillType { get; private set; }

    /// <summary>ShiftCode business attribute.</summary>
    public string ShiftCode { get; private set; }

    /// <summary>ScheduledAtUtc business attribute.</summary>
    public UtcTimestamp? ScheduledAtUtc { get; private set; }

    /// <summary>ParticipationCount business attribute.</summary>
    public int ParticipationCount { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IEmergencyDrillPlanningDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IEmergencyDrillPlanningDomainEvent> DomainEvents
        => new ReadOnlyCollection<IEmergencyDrillPlanningDomainEvent>(_domainEvents);

    // For rehydration only.
    private EmergencyDrillPlanning() 
    {
        // Assigned by the data mapper immediately after construction; see the public
        // constructor for the invariant these properties actually hold.
        DrillType = null!;
        ShiftCode = null!;
    }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new EmergencyDrillPlanning aggregate and raises <see cref="EmergencyDrillPlanningCreatedEvent"/>.</summary>
    public static EmergencyDrillPlanning Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string drillType = "",
        string shiftCode = "",
        UtcTimestamp? scheduledAtUtc = null,
        int participationCount = 0)
    {
        var agg = new EmergencyDrillPlanning
        {
            EmergencyDrillPlanningId     = EmergencyDrillPlanningId.New(),
            TenantId      = tenantId,
            SiteId        = siteId,
            Status        = EmergencyDrillPlanningStatus.Created,
            DrillType = (drillType ?? string.Empty).Trim(),
            ShiftCode = (shiftCode ?? string.Empty).Trim(),
            ScheduledAtUtc = scheduledAtUtc,
            ParticipationCount = participationCount,
            CreatedAtUtc  = createdAtUtc,
            CreatedBy     = createdBy,
        };

        agg.AddDomainEvent(new EmergencyDrillPlanningCreatedEvent(
            EventId:       new EmergencyDrillPlanningEventId(Uuid7.New()),
            EmergencyDrillPlanningId:      agg.EmergencyDrillPlanningId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Lifecycle: Conduct / Complete ---------------------------------------

    /// <summary>
    /// Records that a scheduled drill was conducted (Created -&gt; Active). Valid only from
    /// <see cref="EmergencyDrillPlanningStatus.Created"/>.
    /// </summary>
    public void Conduct(
        UserId conductedBy,
        int participationCount,
        UtcTimestamp conductedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, EmergencyDrillPlanningLifecycleRules.CanActivate, EmergencyDrillPlanningStatus.Active);
        if (participationCount < 0)
            throw new ArgumentOutOfRangeException(nameof(participationCount), participationCount, "ParticipationCount must be non-negative.");

        ParticipationCount = participationCount;
        Status = EmergencyDrillPlanningStatus.Active;

        AddDomainEvent(new EmergencyDrillPlanningConductedEvent(
            EventId: new EmergencyDrillPlanningEventId(Uuid7.New()),
            EmergencyDrillPlanningId: EmergencyDrillPlanningId,
            ConductedBy: conductedBy,
            ParticipationCount: participationCount,
            ConductedAtUtc: conductedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Records the post-drill debrief/completion (Active -&gt; Completed). Valid only from
    /// <see cref="EmergencyDrillPlanningStatus.Active"/>.
    /// </summary>
    public void Complete(
        UserId completedBy,
        string? debriefNotes,
        UtcTimestamp completedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, EmergencyDrillPlanningLifecycleRules.CanComplete, EmergencyDrillPlanningStatus.Completed);

        Status = EmergencyDrillPlanningStatus.Completed;

        AddDomainEvent(new EmergencyDrillPlanningCompletedEvent(
            EventId: new EmergencyDrillPlanningEventId(Uuid7.New()),
            EmergencyDrillPlanningId: EmergencyDrillPlanningId,
            CompletedBy: completedBy,
            DebriefNotes: string.IsNullOrWhiteSpace(debriefNotes) ? null : debriefNotes.Trim(),
            CompletedAtUtc: completedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Close / Archive -------------------------------------------

    /// <summary>
    /// Closes a completed drill record (Completed -&gt; Closed). Valid only from
    /// <see cref="EmergencyDrillPlanningStatus.Completed"/>.
    /// </summary>
    public void Close(
        UserId closedBy,
        UtcTimestamp closedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, EmergencyDrillPlanningLifecycleRules.CanClose, EmergencyDrillPlanningStatus.Closed);

        Status = EmergencyDrillPlanningStatus.Closed;

        AddDomainEvent(new EmergencyDrillPlanningClosedEvent(
            EventId: new EmergencyDrillPlanningEventId(Uuid7.New()),
            EmergencyDrillPlanningId: EmergencyDrillPlanningId,
            ClosedBy: closedBy,
            ClosedAtUtc: closedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Archives a closed drill record for compliance retention (Closed -&gt; Archived). Valid only
    /// from <see cref="EmergencyDrillPlanningStatus.Closed"/>.
    /// </summary>
    public void Archive(
        UtcTimestamp archivedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, EmergencyDrillPlanningLifecycleRules.CanArchive, EmergencyDrillPlanningStatus.Archived);

        Status = EmergencyDrillPlanningStatus.Archived;

        AddDomainEvent(new EmergencyDrillPlanningArchivedEvent(
            EventId: new EmergencyDrillPlanningEventId(Uuid7.New()),
            EmergencyDrillPlanningId: EmergencyDrillPlanningId,
            ArchivedAtUtc: archivedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IEmergencyDrillPlanningDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params EmergencyDrillPlanningStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for EmergencyDrillPlanning {EmergencyDrillPlanningId}.");
    }

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, Func<EmergencyDrillPlanningStatus, bool> guard, EmergencyDrillPlanningStatus expected)
    {
        if (guard(Status)) return;

        throw new EmergencyDrillPlanningInvalidStatusTransitionException(
            EmergencyDrillPlanningId, Status.ToString(), expected.ToString(), correlationId, causationId);
    }
}