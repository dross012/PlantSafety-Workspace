// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.MachineGuarding
//  Layer:          Domain
//  File:           MachineGuarding.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Aggregate root for a single machine guard/barrier/interlock control installed against
//                  a piece of equipment. Machine guarding prevents workers from contacting hazardous
//                  moving parts, pinch points, sharp edges, and high-energy mechanisms via fixed guards,
//                  interlocked doors, adjustable guards, and presence-sensing devices. Models the real
//                  inspection lifecycle: installation, scheduled inspection, pass/fail results, bypass
//                  detection, corrective action, and retirement.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.MachineGuarding.Aggregates;

/// <summary>
/// Aggregate root for a machine guard / barrier / interlock control. Machine guarding is the first line
/// of defense against mechanical hazards: fixed guards, interlocked doors, adjustable guards, and
/// presence-sensing devices that prevent workers from entering danger zones during machine operation.
/// </summary>
public sealed partial class MachineGuarding
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this MachineGuarding aggregate.</summary>
    public MachineGuardingId MachineGuardingId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public MachineGuardingStatus Status { get; private set; }

    // ---- Guard identity & classification ------------------------------------

    /// <summary>Human-readable title/description of this guard (e.g. "Press Brake #3 Interlocked Door").</summary>
    public string GuardTitle { get; private set; } = string.Empty;

    /// <summary>Guard control type: FixedGuard, InterlockedDoor, AdjustableGuard, or PresenceSensingDevice.</summary>
    public MachineGuardType GuardType { get; private set; }

    /// <summary>Asset tag / equipment identifier of the machine this guard protects.</summary>
    public string EquipmentTag { get; private set; } = string.Empty;

    /// <summary>Physical location of the guarded machine (building/line/bay).</summary>
    public string Location { get; private set; } = string.Empty;

    /// <summary>Whether this guard incorporates an interlock that disables the machine when opened.</summary>
    public bool HasInterlock { get; private set; }

    // ---- Inspection state ---------------------------------------------------

    /// <summary>Required inspection cadence, in days, for this guard.</summary>
    public int InspectionIntervalDays { get; private set; } = 90;

    /// <summary>Identity of the user who most recently inspected this guard.</summary>
    public UserId? LastInspectedByUserId { get; private set; }

    /// <summary>UTC timestamp of the most recent inspection, if any.</summary>
    public UtcTimestamp? LastInspectedAtUtc { get; private set; }

    /// <summary>UTC timestamp the next inspection is due.</summary>
    public UtcTimestamp? NextInspectionDueAtUtc { get; private set; }

    /// <summary>Count of open deficiencies recorded against this guard.</summary>
    public int OpenDeficiencyCount { get; private set; }

    /// <summary>Guard condition score (0-100) from the most recent inspection.</summary>
    public decimal? GuardConditionScore { get; private set; }

    /// <summary>True once a bypass/removal has been detected and not yet corrected.</summary>
    public bool BypassDetected { get; private set; }

    /// <summary>Narrative notes describing the most recent finding.</summary>
    public string? LastFindingNotes { get; private set; }

    // ---- Audit ---------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IMachineGuardingDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IMachineGuardingDomainEvent> DomainEvents
        => new ReadOnlyCollection<IMachineGuardingDomainEvent>(_domainEvents);

    // For rehydration only.
    private MachineGuarding() { }

    // ---- Factory: Install ----------------------------------------------------

    /// <summary>Records the installation of a new machine guard and raises <see cref="MachineGuardingInstalledEvent"/>.</summary>
    public static MachineGuarding Install(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        string guardTitle,
        MachineGuardType guardType,
        string equipmentTag,
        string location,
        bool hasInterlock,
        int inspectionIntervalDays = 90)
    {
        if (string.IsNullOrWhiteSpace(guardTitle))
            throw new ArgumentException("Guard title is required.", nameof(guardTitle));
        if (string.IsNullOrWhiteSpace(equipmentTag))
            throw new ArgumentException("Equipment tag is required.", nameof(equipmentTag));
        if (inspectionIntervalDays <= 0)
            throw new ArgumentOutOfRangeException(nameof(inspectionIntervalDays), "Inspection interval must be positive.");

        var agg = new MachineGuarding
        {
            MachineGuardingId       = MachineGuardingId.New(),
            TenantId                = tenantId,
            SiteId                  = siteId,
            Status                  = MachineGuardingStatus.Installed,
            GuardTitle              = guardTitle.Trim(),
            GuardType               = guardType,
            EquipmentTag            = equipmentTag.Trim(),
            Location                = (location ?? string.Empty).Trim(),
            HasInterlock            = hasInterlock,
            InspectionIntervalDays  = inspectionIntervalDays,
            NextInspectionDueAtUtc  = new UtcTimestamp(createdAtUtc.Value.AddDays(inspectionIntervalDays)),
            CreatedAtUtc            = createdAtUtc,
            CreatedBy               = createdBy,
        };

        agg.AddDomainEvent(new MachineGuardingInstalledEvent(
            MachineGuardingEventId.New(), agg.MachineGuardingId, guardType, equipmentTag.Trim(),
            createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: ScheduleInspection ----------------------------------------

    /// <summary>Schedules the next inspection for this guard. Allowed from Installed, Compliant, or InspectionFailed.</summary>
    public void ScheduleInspection(UtcTimestamp dueAtUtc, UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(MachineGuardingStatus.Installed, MachineGuardingStatus.Compliant, MachineGuardingStatus.InspectionFailed);

        NextInspectionDueAtUtc = dueAtUtc;
        Status = MachineGuardingStatus.InspectionScheduled;

        AddDomainEvent(new MachineGuardingInspectionScheduledEvent(
            MachineGuardingEventId.New(), MachineGuardingId, dueAtUtc, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: RecordInspectionResult -------------------------------------

    /// <summary>Records the outcome of a scheduled inspection. Only valid while InspectionScheduled.</summary>
    public void RecordInspectionResult(
        bool passed,
        int deficiencyCount,
        decimal conditionScore,
        UserId inspectedByUserId,
        UtcTimestamp inspectedAtUtc,
        string? notes,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(MachineGuardingStatus.InspectionScheduled);

        if (conditionScore is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(conditionScore), "Guard condition score must be between 0 and 100.");
        if (deficiencyCount < 0)
            throw new ArgumentOutOfRangeException(nameof(deficiencyCount), "Deficiency count cannot be negative.");

        LastInspectedByUserId  = inspectedByUserId;
        LastInspectedAtUtc     = inspectedAtUtc;
        OpenDeficiencyCount    = deficiencyCount;
        GuardConditionScore    = conditionScore;
        LastFindingNotes       = notes;
        NextInspectionDueAtUtc = new UtcTimestamp(inspectedAtUtc.Value.AddDays(InspectionIntervalDays));

        if (passed && deficiencyCount == 0)
        {
            Status = MachineGuardingStatus.Compliant;
            AddDomainEvent(new MachineGuardingInspectionPassedEvent(
                MachineGuardingEventId.New(), MachineGuardingId, conditionScore, inspectedAtUtc, actor, correlationId, causationId));
        }
        else
        {
            Status = MachineGuardingStatus.InspectionFailed;
            AddDomainEvent(new MachineGuardingInspectionFailedEvent(
                MachineGuardingEventId.New(), MachineGuardingId, deficiencyCount, notes ?? string.Empty, inspectedAtUtc, actor, correlationId, causationId));
        }
    }

    // ---- Behavior: FlagBypass --------------------------------------------------

    /// <summary>Flags that the guard/interlock has been found bypassed, defeated, or removed. May occur from any non-retired state.</summary>
    public void FlagBypass(string findingNotes, UserId detectedByUserId, UtcTimestamp detectedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status == MachineGuardingStatus.Retired)
            throw new MachineGuardingInvalidStatusTransitionException(MachineGuardingId, Status.ToString(), MachineGuardingStatus.BypassDetected.ToString(), correlationId, causationId);

        BypassDetected   = true;
        LastFindingNotes = findingNotes;
        Status           = MachineGuardingStatus.BypassDetected;

        AddDomainEvent(new MachineGuardingBypassDetectedEvent(
            MachineGuardingEventId.New(), MachineGuardingId, findingNotes, detectedByUserId, detectedAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: ScheduleCorrectiveAction ------------------------------------

    /// <summary>Opens a corrective-action cycle to remediate a failed inspection or detected bypass.</summary>
    public void ScheduleCorrectiveAction(UtcTimestamp dueAtUtc, UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(MachineGuardingStatus.InspectionFailed, MachineGuardingStatus.BypassDetected);

        Status = MachineGuardingStatus.CorrectiveActionInProgress;

        AddDomainEvent(new MachineGuardingCorrectiveActionScheduledEvent(
            MachineGuardingEventId.New(), MachineGuardingId, dueAtUtc, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: VerifyCorrectiveAction ---------------------------------------

    /// <summary>Verifies corrective action is complete, clearing deficiencies/bypass and returning the guard to service.</summary>
    public void VerifyCorrectiveAction(UserId verifiedByUserId, UtcTimestamp verifiedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(MachineGuardingStatus.CorrectiveActionInProgress);

        OpenDeficiencyCount     = 0;
        BypassDetected          = false;
        Status                  = MachineGuardingStatus.Compliant;
        NextInspectionDueAtUtc  = new UtcTimestamp(verifiedAtUtc.Value.AddDays(InspectionIntervalDays));

        AddDomainEvent(new MachineGuardingCorrectiveActionVerifiedEvent(
            MachineGuardingEventId.New(), MachineGuardingId, verifiedByUserId, verifiedAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Retire --------------------------------------------------------

    /// <summary>Retires this guard record (equipment decommissioned or guard replaced).</summary>
    public void Retire(string reason, UtcTimestamp retiredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status == MachineGuardingStatus.Retired)
            throw new MachineGuardingInvalidStatusTransitionException(MachineGuardingId, Status.ToString(), MachineGuardingStatus.Retired.ToString(), correlationId, causationId);

        Status = MachineGuardingStatus.Retired;

        AddDomainEvent(new MachineGuardingRetiredEvent(
            MachineGuardingEventId.New(), MachineGuardingId, reason, retiredAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IMachineGuardingDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params MachineGuardingStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for MachineGuarding {MachineGuardingId}.");
    }
}
