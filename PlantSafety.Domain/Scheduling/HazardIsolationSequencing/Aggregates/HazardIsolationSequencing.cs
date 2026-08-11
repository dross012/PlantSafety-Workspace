// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.HazardIsolationSequencing
//  Layer:          Domain
//  File:           HazardIsolationSequencing.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the HazardIsolationSequencing bounded context (module: Scheduling).
//                  Additive-only evolution.
//  UPDATED:        2026-07-30 -- Phase 4 full build-out: added Activate/FlagConflict/Complete/
//                  Close/Archive lifecycle methods, wiring the pre-existing (previously unused)
//                  HazardIsolationSequencingLifecycleRules.Can* guards (plus a new CanFlagConflict
//                  guard). Mirrors PlantSafety.Domain.Compliance.Permits.Aggregates.Permits's
//                  conventions.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.HazardIsolationSequencing.Aggregates;

/// <summary>Aggregate root for the HazardIsolationSequencing bounded context.</summary>
public sealed partial class HazardIsolationSequencing
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this HazardIsolationSequencing aggregate.</summary>
    public HazardIsolationSequencingId HazardIsolationSequencingId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public HazardIsolationSequencingStatus Status { get; private set; }

    // ---- HazardIsolationSequencing-specific business properties --------------------------------

    /// <summary>HazardZone business attribute.</summary>
    public string HazardZone { get; private set; }

    /// <summary>TaskType business attribute.</summary>
    public string TaskType { get; private set; }

    /// <summary>WindowStartUtc business attribute.</summary>
    public UtcTimestamp? WindowStartUtc { get; private set; }

    /// <summary>WindowEndUtc business attribute.</summary>
    public UtcTimestamp? WindowEndUtc { get; private set; }

    /// <summary>ConflictDetected business attribute.</summary>
    public bool ConflictDetected { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IHazardIsolationSequencingDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IHazardIsolationSequencingDomainEvent> DomainEvents
        => new ReadOnlyCollection<IHazardIsolationSequencingDomainEvent>(_domainEvents);

    // For rehydration only.
    private HazardIsolationSequencing() 
    {
        // Assigned by the data mapper immediately after construction; see the public
        // constructor for the invariant these properties actually hold.
        HazardZone = null!;
        TaskType = null!;
    }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new HazardIsolationSequencing aggregate and raises <see cref="HazardIsolationSequencingCreatedEvent"/>.</summary>
    public static HazardIsolationSequencing Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string hazardZone = "",
        string taskType = "",
        UtcTimestamp? windowStartUtc = null,
        UtcTimestamp? windowEndUtc = null,
        bool conflictDetected = false)
    {
        var agg = new HazardIsolationSequencing
        {
            HazardIsolationSequencingId     = HazardIsolationSequencingId.New(),
            TenantId      = tenantId,
            SiteId        = siteId,
            Status        = HazardIsolationSequencingStatus.Created,
            HazardZone = (hazardZone ?? string.Empty).Trim(),
            TaskType = (taskType ?? string.Empty).Trim(),
            WindowStartUtc = windowStartUtc,
            WindowEndUtc = windowEndUtc,
            ConflictDetected = conflictDetected,
            CreatedAtUtc  = createdAtUtc,
            CreatedBy     = createdBy,
        };

        agg.AddDomainEvent(new HazardIsolationSequencingCreatedEvent(
            EventId:       new HazardIsolationSequencingEventId(Uuid7.New()),
            HazardIsolationSequencingId:      agg.HazardIsolationSequencingId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Lifecycle: Activate / FlagConflict ----------------------------------

    /// <summary>
    /// Activates the isolation window (Created -&gt; Active). Valid only from
    /// <see cref="HazardIsolationSequencingStatus.Created"/>.
    /// </summary>
    public void Activate(
        UserId activatedBy,
        UtcTimestamp activatedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, HazardIsolationSequencingLifecycleRules.CanActivate, HazardIsolationSequencingStatus.Active);

        Status = HazardIsolationSequencingStatus.Active;

        AddDomainEvent(new HazardIsolationSequencingActivatedEvent(
            EventId: new HazardIsolationSequencingEventId(Uuid7.New()),
            HazardIsolationSequencingId: HazardIsolationSequencingId,
            ActivatedBy: activatedBy,
            ActivatedAtUtc: activatedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Flags an incompatible-task conflict within an active isolation window. Does not change
    /// lifecycle status. Valid only while <see cref="HazardIsolationSequencingStatus.Active"/>.
    /// </summary>
    public void FlagConflict(
        UserId flaggedBy,
        string reason,
        UtcTimestamp flaggedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, HazardIsolationSequencingLifecycleRules.CanFlagConflict, HazardIsolationSequencingStatus.Active);
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("A conflict reason must be supplied.", nameof(reason));

        ConflictDetected = true;

        AddDomainEvent(new HazardIsolationSequencingConflictFlaggedEvent(
            EventId: new HazardIsolationSequencingEventId(Uuid7.New()),
            HazardIsolationSequencingId: HazardIsolationSequencingId,
            FlaggedBy: flaggedBy,
            Reason: reason.Trim(),
            FlaggedAtUtc: flaggedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Complete / Close / Archive -------------------------------

    /// <summary>
    /// Completes the isolation window (Active -&gt; Completed). Valid only from
    /// <see cref="HazardIsolationSequencingStatus.Active"/>.
    /// </summary>
    public void Complete(
        UserId completedBy,
        UtcTimestamp completedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, HazardIsolationSequencingLifecycleRules.CanComplete, HazardIsolationSequencingStatus.Completed);

        Status = HazardIsolationSequencingStatus.Completed;

        AddDomainEvent(new HazardIsolationSequencingCompletedEvent(
            EventId: new HazardIsolationSequencingEventId(Uuid7.New()),
            HazardIsolationSequencingId: HazardIsolationSequencingId,
            CompletedBy: completedBy,
            CompletedAtUtc: completedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Closes a completed isolation record (Completed -&gt; Closed). Valid only from
    /// <see cref="HazardIsolationSequencingStatus.Completed"/>.
    /// </summary>
    public void Close(
        UserId closedBy,
        UtcTimestamp closedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, HazardIsolationSequencingLifecycleRules.CanClose, HazardIsolationSequencingStatus.Closed);

        Status = HazardIsolationSequencingStatus.Closed;

        AddDomainEvent(new HazardIsolationSequencingClosedEvent(
            EventId: new HazardIsolationSequencingEventId(Uuid7.New()),
            HazardIsolationSequencingId: HazardIsolationSequencingId,
            ClosedBy: closedBy,
            ClosedAtUtc: closedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Archives a closed isolation record for compliance retention (Closed -&gt; Archived). Valid
    /// only from <see cref="HazardIsolationSequencingStatus.Closed"/>.
    /// </summary>
    public void Archive(
        UtcTimestamp archivedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, HazardIsolationSequencingLifecycleRules.CanArchive, HazardIsolationSequencingStatus.Archived);

        Status = HazardIsolationSequencingStatus.Archived;

        AddDomainEvent(new HazardIsolationSequencingArchivedEvent(
            EventId: new HazardIsolationSequencingEventId(Uuid7.New()),
            HazardIsolationSequencingId: HazardIsolationSequencingId,
            ArchivedAtUtc: archivedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IHazardIsolationSequencingDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params HazardIsolationSequencingStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for HazardIsolationSequencing {HazardIsolationSequencingId}.");
    }

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, Func<HazardIsolationSequencingStatus, bool> guard, HazardIsolationSequencingStatus expected)
    {
        if (guard(Status)) return;

        throw new HazardIsolationSequencingInvalidStatusTransitionException(
            HazardIsolationSequencingId, Status.ToString(), expected.ToString(), correlationId, causationId);
    }
}