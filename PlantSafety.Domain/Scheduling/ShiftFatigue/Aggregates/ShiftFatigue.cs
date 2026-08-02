// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.ShiftFatigue
//  Layer:          Domain
//  File:           ShiftFatigue.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the ShiftFatigue bounded context (module: Scheduling).
//                  Additive-only evolution.
//  UPDATED:        2026-07-30 -- Phase 4 full build-out: added Activate/ReassessRisk/Complete/
//                  Close/Archive lifecycle methods, wiring the pre-existing (previously unused)
//                  ShiftFatigueLifecycleRules.Can* guards (plus a new CanReassessRisk guard).
//                  Mirrors PlantSafety.Domain.Compliance.Permits.Aggregates.Permits's conventions.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.ShiftFatigue.Aggregates;

/// <summary>Aggregate root for the ShiftFatigue bounded context.</summary>
public sealed partial class ShiftFatigue
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this ShiftFatigue aggregate.</summary>
    public ShiftFatigueId ShiftFatigueId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public ShiftFatigueStatus Status { get; private set; }

    // ---- ShiftFatigue-specific business properties --------------------------------

    /// <summary>EmployeeId business attribute.</summary>
    public Guid EmployeeId { get; private set; }

    /// <summary>ShiftPattern business attribute.</summary>
    public string ShiftPattern { get; private set; }

    /// <summary>ConsecutiveNightShifts business attribute.</summary>
    public int ConsecutiveNightShifts { get; private set; }

    /// <summary>OvertimeHours business attribute.</summary>
    public decimal OvertimeHours { get; private set; }

    /// <summary>FatigueRiskScore business attribute.</summary>
    public decimal FatigueRiskScore { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IShiftFatigueDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IShiftFatigueDomainEvent> DomainEvents
        => new ReadOnlyCollection<IShiftFatigueDomainEvent>(_domainEvents);

    // For rehydration only.
    private ShiftFatigue() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new ShiftFatigue aggregate and raises <see cref="ShiftFatigueCreatedEvent"/>.</summary>
    public static ShiftFatigue Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        Guid employeeId = default,
        string shiftPattern = "",
        int consecutiveNightShifts = 0,
        decimal overtimeHours = 0m,
        decimal fatigueRiskScore = 0m)
    {
        var agg = new ShiftFatigue
        {
            ShiftFatigueId     = ShiftFatigueId.New(),
            TenantId      = tenantId,
            SiteId        = siteId,
            Status        = ShiftFatigueStatus.Created,
            EmployeeId = employeeId,
            ShiftPattern = (shiftPattern ?? string.Empty).Trim(),
            ConsecutiveNightShifts = consecutiveNightShifts,
            OvertimeHours = overtimeHours,
            FatigueRiskScore = fatigueRiskScore,
            CreatedAtUtc  = createdAtUtc,
            CreatedBy     = createdBy,
        };

        agg.AddDomainEvent(new ShiftFatigueCreatedEvent(
            EventId:       new ShiftFatigueEventId(Uuid7.New()),
            ShiftFatigueId:      agg.ShiftFatigueId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Lifecycle: Activate / ReassessRisk ----------------------------------

    /// <summary>
    /// Activates fatigue monitoring for this employee's shift record (Created -&gt; Active). Valid
    /// only from <see cref="ShiftFatigueStatus.Created"/>.
    /// </summary>
    public void Activate(
        UserId activatedBy,
        UtcTimestamp activatedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, ShiftFatigueLifecycleRules.CanActivate, ShiftFatigueStatus.Active);

        Status = ShiftFatigueStatus.Active;

        AddDomainEvent(new ShiftFatigueActivatedEvent(
            EventId: new ShiftFatigueEventId(Uuid7.New()),
            ShiftFatigueId: ShiftFatigueId,
            ActivatedBy: activatedBy,
            ActivatedAtUtc: activatedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Reassesses the fatigue risk score during active monitoring. Does not change lifecycle
    /// status. Valid only while <see cref="ShiftFatigueStatus.Active"/>.
    /// </summary>
    public void ReassessRisk(
        UserId reassessedBy,
        decimal fatigueRiskScore,
        int consecutiveNightShifts,
        decimal overtimeHours,
        UtcTimestamp reassessedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, ShiftFatigueLifecycleRules.CanReassessRisk, ShiftFatigueStatus.Active);
        if (fatigueRiskScore < 0)
            throw new ArgumentOutOfRangeException(nameof(fatigueRiskScore), fatigueRiskScore, "FatigueRiskScore must be non-negative.");
        if (consecutiveNightShifts < 0)
            throw new ArgumentOutOfRangeException(nameof(consecutiveNightShifts), consecutiveNightShifts, "ConsecutiveNightShifts must be non-negative.");
        if (overtimeHours < 0)
            throw new ArgumentOutOfRangeException(nameof(overtimeHours), overtimeHours, "OvertimeHours must be non-negative.");

        FatigueRiskScore = fatigueRiskScore;
        ConsecutiveNightShifts = consecutiveNightShifts;
        OvertimeHours = overtimeHours;

        AddDomainEvent(new ShiftFatigueRiskReassessedEvent(
            EventId: new ShiftFatigueEventId(Uuid7.New()),
            ShiftFatigueId: ShiftFatigueId,
            ReassessedBy: reassessedBy,
            FatigueRiskScore: fatigueRiskScore,
            ConsecutiveNightShifts: consecutiveNightShifts,
            OvertimeHours: overtimeHours,
            ReassessedAtUtc: reassessedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Complete / Close / Archive -------------------------------

    /// <summary>
    /// Completes the monitoring period (Active -&gt; Completed). Valid only from
    /// <see cref="ShiftFatigueStatus.Active"/>.
    /// </summary>
    public void Complete(
        UserId completedBy,
        UtcTimestamp completedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, ShiftFatigueLifecycleRules.CanComplete, ShiftFatigueStatus.Completed);

        Status = ShiftFatigueStatus.Completed;

        AddDomainEvent(new ShiftFatigueCompletedEvent(
            EventId: new ShiftFatigueEventId(Uuid7.New()),
            ShiftFatigueId: ShiftFatigueId,
            CompletedBy: completedBy,
            CompletedAtUtc: completedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Closes a completed fatigue record (Completed -&gt; Closed). Valid only from
    /// <see cref="ShiftFatigueStatus.Completed"/>.
    /// </summary>
    public void Close(
        UserId closedBy,
        UtcTimestamp closedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, ShiftFatigueLifecycleRules.CanClose, ShiftFatigueStatus.Closed);

        Status = ShiftFatigueStatus.Closed;

        AddDomainEvent(new ShiftFatigueClosedEvent(
            EventId: new ShiftFatigueEventId(Uuid7.New()),
            ShiftFatigueId: ShiftFatigueId,
            ClosedBy: closedBy,
            ClosedAtUtc: closedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Archives a closed fatigue record for compliance retention (Closed -&gt; Archived). Valid
    /// only from <see cref="ShiftFatigueStatus.Closed"/>.
    /// </summary>
    public void Archive(
        UtcTimestamp archivedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, ShiftFatigueLifecycleRules.CanArchive, ShiftFatigueStatus.Archived);

        Status = ShiftFatigueStatus.Archived;

        AddDomainEvent(new ShiftFatigueArchivedEvent(
            EventId: new ShiftFatigueEventId(Uuid7.New()),
            ShiftFatigueId: ShiftFatigueId,
            ArchivedAtUtc: archivedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IShiftFatigueDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params ShiftFatigueStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for ShiftFatigue {ShiftFatigueId}.");
    }

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, Func<ShiftFatigueStatus, bool> guard, ShiftFatigueStatus expected)
    {
        if (guard(Status)) return;

        throw new ShiftFatigueInvalidStatusTransitionException(
            ShiftFatigueId, Status.ToString(), expected.ToString(), correlationId, causationId);
    }
}