// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.SafetyValvesRelays
//  Layer:          Domain
//  File:           SafetyValvesRelays.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Aggregate root for a single safety valve/relay component that ensures machines fail
//                  safely during malfunctions: emergency shut-off valves, dual-channel safety relays,
//                  and redundant control systems. Models the real reliability lifecycle: installation,
//                  scheduled inspection, reliable/fault outcome, repair, and retirement.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.SafetyValvesRelays.Aggregates;

/// <summary>Aggregate root for a single safety valve/relay component.</summary>
public sealed partial class SafetyValvesRelays
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this SafetyValvesRelays aggregate.</summary>
    public SafetyValvesRelaysId SafetyValvesRelaysId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public SafetyValvesRelaysStatus Status { get; private set; }

    // ---- Component identity ---------------------------------------------------------

    /// <summary>Human-readable title of this component.</summary>
    public string ComponentTitle { get; private set; } = string.Empty;

    /// <summary>Safety component type.</summary>
    public SafetyComponentType ComponentType { get; private set; }

    /// <summary>Location of the protected equipment.</summary>
    public string EquipmentLocation { get; private set; } = string.Empty;

    /// <summary>Identity of the employee who last inspected this component.</summary>
    public UserId InspectedByUserId { get; private set; }

    /// <summary>UTC timestamp of the most recent inspection, if any.</summary>
    public UtcTimestamp? InspectedAtUtc { get; private set; }

    /// <summary>UTC timestamp the next inspection is due.</summary>
    public UtcTimestamp? NextInspectionDueAtUtc { get; private set; }

    /// <summary>Cumulative count of faults detected.</summary>
    public int FaultCount { get; private set; }

    /// <summary>Reliability score (0-100) from the most recent inspection.</summary>
    public decimal? ReliabilityScore { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<ISafetyValvesRelaysDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<ISafetyValvesRelaysDomainEvent> DomainEvents
        => new ReadOnlyCollection<ISafetyValvesRelaysDomainEvent>(_domainEvents);

    // For rehydration only.
    private SafetyValvesRelays() { }

    // ---- Factory: Create (Install) -----------------------------------------------------------

    /// <summary>Records installation of a new safety valve/relay component and raises <see cref="SafetyComponentInstalledEvent"/>.</summary>
    public static SafetyValvesRelays Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string componentTitle = "",
        SafetyComponentType componentType = SafetyComponentType.EmergencyShutOffValve,
        string equipmentLocation = "")
    {
        var agg = new SafetyValvesRelays
        {
            SafetyValvesRelaysId = SafetyValvesRelaysId.New(),
            TenantId          = tenantId,
            SiteId            = siteId,
            Status            = SafetyValvesRelaysStatus.Installed,
            ComponentTitle    = (componentTitle ?? string.Empty).Trim(),
            ComponentType     = componentType,
            EquipmentLocation = (equipmentLocation ?? string.Empty).Trim(),
            CreatedAtUtc      = createdAtUtc,
            CreatedBy         = createdBy,
        };

        agg.AddDomainEvent(new SafetyComponentInstalledEvent(
            SafetyValvesRelaysEventId.New(), agg.SafetyValvesRelaysId, componentType, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: ScheduleInspection -----------------------------------------------

    /// <summary>Schedules the next reliability inspection.</summary>
    public void ScheduleInspection(UtcTimestamp dueAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(SafetyValvesRelaysStatus.Installed, SafetyValvesRelaysStatus.Reliable);

        NextInspectionDueAtUtc = dueAtUtc;
        Status = SafetyValvesRelaysStatus.InspectionScheduled;

        AddDomainEvent(new SafetyComponentInspectionScheduledEvent(
            SafetyValvesRelaysEventId.New(), SafetyValvesRelaysId, dueAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: RecordInspectionResult -----------------------------------------------

    /// <summary>Records the outcome of a scheduled inspection.</summary>
    public void RecordInspectionResult(bool reliable, decimal reliabilityScore, UserId inspectedByUserId, UtcTimestamp inspectedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(SafetyValvesRelaysStatus.InspectionScheduled);

        if (reliabilityScore is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(reliabilityScore), "Reliability score must be between 0 and 100.");

        InspectedByUserId = inspectedByUserId;
        InspectedAtUtc     = inspectedAtUtc;
        ReliabilityScore   = reliabilityScore;

        if (reliable)
        {
            Status = SafetyValvesRelaysStatus.Reliable;
            AddDomainEvent(new SafetyComponentReliableEvent(
                SafetyValvesRelaysEventId.New(), SafetyValvesRelaysId, reliabilityScore, inspectedAtUtc, actor, correlationId, causationId));
        }
        else
        {
            FaultCount++;
            Status = SafetyValvesRelaysStatus.FaultDetected;
            AddDomainEvent(new SafetyComponentFaultDetectedEvent(
                SafetyValvesRelaysEventId.New(), SafetyValvesRelaysId, FaultCount, inspectedAtUtc, actor, correlationId, causationId));
        }
    }

    // ---- Behavior: BeginRepair -----------------------------------------------

    /// <summary>Begins repair after a fault is detected.</summary>
    public void BeginRepair(UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(SafetyValvesRelaysStatus.FaultDetected);

        Status = SafetyValvesRelaysStatus.RepairInProgress;

        AddDomainEvent(new SafetyComponentRepairStartedEvent(
            SafetyValvesRelaysEventId.New(), SafetyValvesRelaysId, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Retire -----------------------------------------------------------

    /// <summary>Retires this component.</summary>
    public void Retire(UtcTimestamp retiredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status == SafetyValvesRelaysStatus.Retired)
            throw new SafetyValvesRelaysInvalidStatusTransitionException(SafetyValvesRelaysId, Status.ToString(), SafetyValvesRelaysStatus.Retired.ToString(), correlationId, causationId);

        Status = SafetyValvesRelaysStatus.Retired;

        AddDomainEvent(new SafetyComponentRetiredEvent(
            SafetyValvesRelaysEventId.New(), SafetyValvesRelaysId, retiredAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(ISafetyValvesRelaysDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params SafetyValvesRelaysStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for SafetyValvesRelays {SafetyValvesRelaysId}.");
    }
}
