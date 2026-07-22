// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.MachineGuardsBarriers
//  Layer:          Domain
//  File:           MachineGuardsBarriers.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Aggregate root for a single machine guard/barrier physical safeguard installed on
//                  equipment: fixed guards, interlocked doors, physical enclosures, and adjustable
//                  guards. Models the real inspection lifecycle: installation, scheduled inspection,
//                  compliance/deficiency outcome, corrective action, and retirement.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.MachineGuardsBarriers.Aggregates;

/// <summary>Aggregate root for a single machine guard/barrier physical safeguard.</summary>
public sealed partial class MachineGuardsBarriers
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this MachineGuardsBarriers aggregate.</summary>
    public MachineGuardsBarriersId MachineGuardsBarriersId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public MachineGuardsBarriersStatus Status { get; private set; }

    // ---- Guard identity ---------------------------------------------------------

    /// <summary>Human-readable title of this guard/barrier.</summary>
    public string GuardTitle { get; private set; } = string.Empty;

    /// <summary>Guard/barrier type.</summary>
    public MachineGuardBarrierType GuardType { get; private set; }

    /// <summary>Location of the guarded equipment.</summary>
    public string EquipmentLocation { get; private set; } = string.Empty;

    /// <summary>Identity of the employee who installed this guard.</summary>
    public UserId InstalledByUserId { get; private set; }

    /// <summary>UTC timestamp of the most recent inspection, if any.</summary>
    public UtcTimestamp? InspectedAtUtc { get; private set; }

    /// <summary>UTC timestamp the next inspection is due.</summary>
    public UtcTimestamp? NextInspectionDueAtUtc { get; private set; }

    /// <summary>Count of open deficiencies.</summary>
    public int DeficiencyCount { get; private set; }

    /// <summary>Guard integrity score (0-100) from the most recent inspection.</summary>
    public decimal? GuardIntegrityScore { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IMachineGuardsBarriersDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IMachineGuardsBarriersDomainEvent> DomainEvents
        => new ReadOnlyCollection<IMachineGuardsBarriersDomainEvent>(_domainEvents);

    // For rehydration only.
    private MachineGuardsBarriers() { }

    // ---- Factory: Create (Install) -----------------------------------------------------------

    /// <summary>Records installation of a new guard/barrier and raises <see cref="MachineGuardsBarriersInstalledEvent"/>.</summary>
    public static MachineGuardsBarriers Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string guardTitle = "",
        MachineGuardBarrierType guardType = MachineGuardBarrierType.FixedGuard,
        string equipmentLocation = "",
        UserId installedByUserId = default)
    {
        var agg = new MachineGuardsBarriers
        {
            MachineGuardsBarriersId = MachineGuardsBarriersId.New(),
            TenantId           = tenantId,
            SiteId             = siteId,
            Status             = MachineGuardsBarriersStatus.Installed,
            GuardTitle         = (guardTitle ?? string.Empty).Trim(),
            GuardType          = guardType,
            EquipmentLocation  = (equipmentLocation ?? string.Empty).Trim(),
            InstalledByUserId  = installedByUserId,
            CreatedAtUtc       = createdAtUtc,
            CreatedBy          = createdBy,
        };

        agg.AddDomainEvent(new MachineGuardsBarriersInstalledEvent(
            MachineGuardsBarriersEventId.New(), agg.MachineGuardsBarriersId, guardType, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: ScheduleInspection -----------------------------------------------

    /// <summary>Schedules the next inspection.</summary>
    public void ScheduleInspection(UtcTimestamp dueAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(MachineGuardsBarriersStatus.Installed, MachineGuardsBarriersStatus.Compliant);

        NextInspectionDueAtUtc = dueAtUtc;
        Status = MachineGuardsBarriersStatus.InspectionScheduled;

        AddDomainEvent(new MachineGuardsBarriersInspectionScheduledEvent(
            MachineGuardsBarriersEventId.New(), MachineGuardsBarriersId, dueAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: RecordInspectionResult -----------------------------------------------

    /// <summary>Records the outcome of a scheduled inspection.</summary>
    public void RecordInspectionResult(int deficiencyCount, decimal guardIntegrityScore, UtcTimestamp inspectedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(MachineGuardsBarriersStatus.InspectionScheduled);

        if (deficiencyCount < 0)
            throw new ArgumentOutOfRangeException(nameof(deficiencyCount), "Deficiency count cannot be negative.");
        if (guardIntegrityScore is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(guardIntegrityScore), "Guard integrity score must be between 0 and 100.");

        DeficiencyCount     = deficiencyCount;
        GuardIntegrityScore = guardIntegrityScore;
        InspectedAtUtc      = inspectedAtUtc;

        if (deficiencyCount == 0)
        {
            Status = MachineGuardsBarriersStatus.Compliant;
            AddDomainEvent(new MachineGuardsBarriersCompliantEvent(
                MachineGuardsBarriersEventId.New(), MachineGuardsBarriersId, guardIntegrityScore, inspectedAtUtc, actor, correlationId, causationId));
        }
        else
        {
            Status = MachineGuardsBarriersStatus.DeficiencyFound;
            AddDomainEvent(new MachineGuardsBarriersDeficiencyFoundEvent(
                MachineGuardsBarriersEventId.New(), MachineGuardsBarriersId, deficiencyCount, inspectedAtUtc, actor, correlationId, causationId));
        }
    }

    // ---- Behavior: BeginCorrectiveAction -----------------------------------------------

    /// <summary>Begins corrective action to remediate a deficiency.</summary>
    public void BeginCorrectiveAction(UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(MachineGuardsBarriersStatus.DeficiencyFound);

        Status = MachineGuardsBarriersStatus.CorrectiveActionInProgress;

        AddDomainEvent(new MachineGuardsBarriersCorrectiveActionStartedEvent(
            MachineGuardsBarriersEventId.New(), MachineGuardsBarriersId, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: VerifyCorrectiveAction -----------------------------------------------

    /// <summary>Verifies corrective action complete, returning the guard to compliant status.</summary>
    public void VerifyCorrectiveAction(UtcTimestamp verifiedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(MachineGuardsBarriersStatus.CorrectiveActionInProgress);

        DeficiencyCount = 0;
        Status = MachineGuardsBarriersStatus.Compliant;

        AddDomainEvent(new MachineGuardsBarriersCorrectiveActionVerifiedEvent(
            MachineGuardsBarriersEventId.New(), MachineGuardsBarriersId, verifiedAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Retire -----------------------------------------------------------

    /// <summary>Retires this guard/barrier record.</summary>
    public void Retire(UtcTimestamp retiredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status == MachineGuardsBarriersStatus.Retired)
            throw new MachineGuardsBarriersInvalidStatusTransitionException(MachineGuardsBarriersId, Status.ToString(), MachineGuardsBarriersStatus.Retired.ToString(), correlationId, causationId);

        Status = MachineGuardsBarriersStatus.Retired;

        AddDomainEvent(new MachineGuardsBarriersRetiredEvent(
            MachineGuardsBarriersEventId.New(), MachineGuardsBarriersId, retiredAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IMachineGuardsBarriersDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params MachineGuardsBarriersStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for MachineGuardsBarriers {MachineGuardsBarriersId}.");
    }
}
