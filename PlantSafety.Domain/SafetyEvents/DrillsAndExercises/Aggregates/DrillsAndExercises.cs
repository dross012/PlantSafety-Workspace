// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.DrillsAndExercises
//  Layer:          Domain
//  File:           DrillsAndExercises.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the SafetyEvents.DrillsAndExercises bounded context. Tracks
//                  a single drill/exercise from scheduling through conducting, completion (with
//                  debrief), or cancellation, through closure and archival.
//                  Phase 4 full build-out: this bounded context previously had no aggregate at
//                  all -- only a placeholder marker class (DrillsAndExercisesModuleMarker) and a
//                  read-only list query slice over the [SafetyEvents].[DrillsAndExercises] table
//                  existed. Field shape (DrillType, Status, ConductedAtUtc, ConductedBy,
//                  ParticipantCount) is grounded in that pre-existing read model's actual columns.
//                  ScheduledAtUtc is a legitimate additive write-model field the read DTO does not
//                  yet surface (a drill can be scheduled before it is conducted). Mirrors
//                  PlantSafety.Domain.Compliance.Permits.Aggregates.Permits's guarded-transition/
//                  domain-event/exception conventions.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.SafetyEvents.DrillsAndExercises.Enums;
using PlantSafety.Domain.SafetyEvents.DrillsAndExercises.Events;
using PlantSafety.Domain.SafetyEvents.DrillsAndExercises.Exceptions;
using PlantSafety.Domain.SafetyEvents.DrillsAndExercises.Rules;
using PlantSafety.Domain.SafetyEvents.DrillsAndExercises.ValueObjects.Ids;

namespace PlantSafety.Domain.SafetyEvents.DrillsAndExercises.Aggregates;

/// <summary>Aggregate root for the SafetyEvents.DrillsAndExercises bounded context.</summary>
public sealed class DrillsAndExercises
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this DrillsAndExercises aggregate.</summary>
    public DrillsAndExercisesId DrillsAndExercisesId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public DrillsAndExercisesStatus Status { get; private set; }

    // ---- Drill-specific business properties -----------------------------------

    /// <summary>Type of drill (e.g., FireDrill/Evacuation/ShelterInPlace/SpillResponse).</summary>
    public string DrillType { get; private set; } = string.Empty;

    /// <summary>Number of participants involved in the drill.</summary>
    public int ParticipantCount { get; private set; }

    /// <summary>UTC instant the drill was scheduled for. Null if never scheduled with a target time.</summary>
    public UtcTimestamp? ScheduledAtUtc { get; private set; }

    /// <summary>UTC instant the drill was actually conducted.</summary>
    public UtcTimestamp? ConductedAtUtc { get; private set; }

    /// <summary>Identity of the user who conducted the drill.</summary>
    public UserId? ConductedBy { get; private set; }

    // ---- Lifecycle attributes (Complete/Cancel/Close/Archive) -------------------

    /// <summary>Identity of the user who completed the drill (recorded the debrief).</summary>
    public UserId? CompletedBy { get; private set; }

    /// <summary>UTC instant the drill was completed.</summary>
    public UtcTimestamp? CompletedAtUtc { get; private set; }

    /// <summary>Free-text debrief notes recorded on completion.</summary>
    public string? DebriefNotes { get; private set; }

    /// <summary>Identity of the user who cancelled the drill.</summary>
    public UserId? CancelledBy { get; private set; }

    /// <summary>Reason recorded when a scheduled drill is cancelled.</summary>
    public string? CancelledReason { get; private set; }

    /// <summary>UTC instant the drill was cancelled.</summary>
    public UtcTimestamp? CancelledAtUtc { get; private set; }

    /// <summary>Identity of the user who formally closed the drill record.</summary>
    public UserId? ClosedBy { get; private set; }

    /// <summary>UTC instant the drill record was closed.</summary>
    public UtcTimestamp? ClosedAtUtc { get; private set; }

    /// <summary>UTC instant the drill record was archived.</summary>
    public UtcTimestamp? ArchivedAtUtc { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user/system that created this record.</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events ---------------------------------------------------------

    private readonly List<IDrillsAndExercisesDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IDrillsAndExercisesDomainEvent> DomainEvents
        => new ReadOnlyCollection<IDrillsAndExercisesDomainEvent>(_domainEvents);

    // For rehydration only.
    private DrillsAndExercises() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Schedules a new DrillsAndExercises drill and records <see cref="DrillsAndExercisesScheduledEvent"/>.</summary>
    public static DrillsAndExercises Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        string drillType,
        UtcTimestamp? scheduledAtUtc)
    {
        if (string.IsNullOrWhiteSpace(drillType))
            throw new ArgumentException("DrillType must be supplied.", nameof(drillType));

        var agg = new DrillsAndExercises
        {
            DrillsAndExercisesId = DrillsAndExercisesId.New(),
            TenantId = tenantId,
            SiteId = siteId,
            Status = DrillsAndExercisesStatus.Scheduled,
            DrillType = drillType.Trim(),
            ScheduledAtUtc = scheduledAtUtc,
            ParticipantCount = 0,
            CreatedAtUtc = createdAtUtc,
            CreatedBy = createdBy,
        };

        agg.AddDomainEvent(new DrillsAndExercisesScheduledEvent(
            EventId: DrillsAndExercisesEventId.New(),
            DrillsAndExercisesId: agg.DrillsAndExercisesId,
            TenantId: tenantId,
            SiteId: siteId,
            DrillType: agg.DrillType,
            ScheduledAtUtc: scheduledAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId,
            OccurredAtUtc: scheduledAtUtc ?? createdAtUtc));

        return agg;
    }

    // ---- Lifecycle: Conduct --------------------------------------------------

    /// <summary>
    /// Records that a scheduled drill was conducted (Scheduled -&gt; Conducted). Valid only from
    /// <see cref="DrillsAndExercisesStatus.Scheduled"/>.
    /// </summary>
    public void Conduct(
        UserId conductedBy,
        int participantCount,
        UtcTimestamp conductedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, DrillsAndExercisesLifecycleRules.CanConduct, DrillsAndExercisesStatus.Conducted);
        if (participantCount < 0)
            throw new ArgumentOutOfRangeException(nameof(participantCount), "ParticipantCount must be non-negative.");

        ConductedBy = conductedBy;
        ConductedAtUtc = conductedAtUtc;
        ParticipantCount = participantCount;
        Status = DrillsAndExercisesStatus.Conducted;

        AddDomainEvent(new DrillsAndExercisesConductedEvent(
            EventId: DrillsAndExercisesEventId.New(),
            DrillsAndExercisesId: DrillsAndExercisesId,
            ConductedBy: conductedBy,
            ParticipantCount: participantCount,
            ConductedAtUtc: conductedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Complete -----------------------------------------------------

    /// <summary>
    /// Completes a conducted drill, recording the debrief (Conducted -&gt; Completed). Valid only
    /// from <see cref="DrillsAndExercisesStatus.Conducted"/>.
    /// </summary>
    public void Complete(
        UserId completedBy,
        string? debriefNotes,
        UtcTimestamp completedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, DrillsAndExercisesLifecycleRules.CanComplete, DrillsAndExercisesStatus.Completed);

        CompletedBy = completedBy;
        DebriefNotes = string.IsNullOrWhiteSpace(debriefNotes) ? null : debriefNotes.Trim();
        CompletedAtUtc = completedAtUtc;
        Status = DrillsAndExercisesStatus.Completed;

        AddDomainEvent(new DrillsAndExercisesCompletedEvent(
            EventId: DrillsAndExercisesEventId.New(),
            DrillsAndExercisesId: DrillsAndExercisesId,
            CompletedBy: completedBy,
            DebriefNotes: DebriefNotes,
            CompletedAtUtc: completedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Cancel -----------------------------------------------------

    /// <summary>
    /// Cancels a scheduled drill before it is conducted (Scheduled -&gt; Cancelled). Valid only
    /// from <see cref="DrillsAndExercisesStatus.Scheduled"/>.
    /// </summary>
    public void Cancel(
        UserId cancelledBy,
        string reason,
        UtcTimestamp cancelledAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, DrillsAndExercisesLifecycleRules.CanCancel, DrillsAndExercisesStatus.Cancelled);
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("A cancellation reason must be supplied.", nameof(reason));

        CancelledBy = cancelledBy;
        CancelledReason = reason.Trim();
        CancelledAtUtc = cancelledAtUtc;
        Status = DrillsAndExercisesStatus.Cancelled;

        AddDomainEvent(new DrillsAndExercisesCancelledEvent(
            EventId: DrillsAndExercisesEventId.New(),
            DrillsAndExercisesId: DrillsAndExercisesId,
            CancelledBy: cancelledBy,
            Reason: CancelledReason,
            CancelledAtUtc: cancelledAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Close ------------------------------------------------------

    /// <summary>
    /// Formally closes a completed drill record (Completed -&gt; Closed). Valid only from
    /// <see cref="DrillsAndExercisesStatus.Completed"/>.
    /// </summary>
    public void Close(
        UserId closedBy,
        UtcTimestamp closedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, DrillsAndExercisesLifecycleRules.CanClose, DrillsAndExercisesStatus.Closed);

        ClosedBy = closedBy;
        ClosedAtUtc = closedAtUtc;
        Status = DrillsAndExercisesStatus.Closed;

        AddDomainEvent(new DrillsAndExercisesClosedEvent(
            EventId: DrillsAndExercisesEventId.New(),
            DrillsAndExercisesId: DrillsAndExercisesId,
            ClosedBy: closedBy,
            ClosedAtUtc: closedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Archive -----------------------------------------------------

    /// <summary>
    /// Archives a closed drill record (Closed -&gt; Archived). Valid only from
    /// <see cref="DrillsAndExercisesStatus.Closed"/>.
    /// </summary>
    public void Archive(
        UtcTimestamp archivedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, DrillsAndExercisesLifecycleRules.CanArchive, DrillsAndExercisesStatus.Archived);

        ArchivedAtUtc = archivedAtUtc;
        Status = DrillsAndExercisesStatus.Archived;

        AddDomainEvent(new DrillsAndExercisesArchivedEvent(
            EventId: DrillsAndExercisesEventId.New(),
            DrillsAndExercisesId: DrillsAndExercisesId,
            ArchivedAtUtc: archivedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IDrillsAndExercisesDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, Func<DrillsAndExercisesStatus, bool> guard, DrillsAndExercisesStatus expected)
    {
        if (guard(Status)) return;

        throw new DrillsAndExercisesInvalidStatusTransitionException(
            DrillsAndExercisesId, Status.ToString(), expected.ToString(), correlationId, causationId);
    }
}
