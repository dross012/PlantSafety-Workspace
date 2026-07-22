// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.TwoHandControls
//  Layer:          Domain
//  File:           TwoHandControls.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Aggregate root for a single two-hand control system requiring operators to press
//                  two separate buttons simultaneously to activate machine cycles on presses, cutters,
//                  and forming machines. Models the real test lifecycle: installation, scheduled
//                  synchronization testing, pass/bypass outcome, and retirement.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.TwoHandControls.Aggregates;

/// <summary>Aggregate root for a single two-hand control system.</summary>
public sealed partial class TwoHandControls
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this TwoHandControls aggregate.</summary>
    public TwoHandControlsId TwoHandControlsId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public TwoHandControlsStatus Status { get; private set; }

    // ---- Control identity ---------------------------------------------------------

    /// <summary>Human-readable title of this control.</summary>
    public string ControlTitle { get; private set; } = string.Empty;

    /// <summary>Equipment type this control is installed on.</summary>
    public TwoHandControlEquipmentType EquipmentType { get; private set; }

    /// <summary>Location of the protected equipment.</summary>
    public string EquipmentLocation { get; private set; } = string.Empty;

    /// <summary>Identity of the employee who last tested this control.</summary>
    public UserId TestedByUserId { get; private set; }

    /// <summary>UTC timestamp of the most recent test, if any.</summary>
    public UtcTimestamp? TestedAtUtc { get; private set; }

    /// <summary>UTC timestamp the next test is due.</summary>
    public UtcTimestamp? NextTestDueAtUtc { get; private set; }

    /// <summary>Cumulative count of bypass attempts detected.</summary>
    public int BypassAttemptCount { get; private set; }

    /// <summary>Synchronization score (0-100) from the most recent test.</summary>
    public decimal? SynchronizationScore { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<ITwoHandControlsDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<ITwoHandControlsDomainEvent> DomainEvents
        => new ReadOnlyCollection<ITwoHandControlsDomainEvent>(_domainEvents);

    // For rehydration only.
    private TwoHandControls() { }

    // ---- Factory: Create (Install) -----------------------------------------------------------

    /// <summary>Records installation of a new two-hand control and raises <see cref="TwoHandControlInstalledEvent"/>.</summary>
    public static TwoHandControls Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string controlTitle = "",
        TwoHandControlEquipmentType equipmentType = TwoHandControlEquipmentType.Press,
        string equipmentLocation = "")
    {
        var agg = new TwoHandControls
        {
            TwoHandControlsId = TwoHandControlsId.New(),
            TenantId          = tenantId,
            SiteId            = siteId,
            Status            = TwoHandControlsStatus.Installed,
            ControlTitle      = (controlTitle ?? string.Empty).Trim(),
            EquipmentType     = equipmentType,
            EquipmentLocation = (equipmentLocation ?? string.Empty).Trim(),
            CreatedAtUtc      = createdAtUtc,
            CreatedBy         = createdBy,
        };

        agg.AddDomainEvent(new TwoHandControlInstalledEvent(
            TwoHandControlsEventId.New(), agg.TwoHandControlsId, equipmentType, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: ScheduleTest -----------------------------------------------

    /// <summary>Schedules the next synchronization test.</summary>
    public void ScheduleTest(UtcTimestamp dueAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(TwoHandControlsStatus.Installed, TwoHandControlsStatus.Synchronized);

        NextTestDueAtUtc = dueAtUtc;
        Status = TwoHandControlsStatus.TestScheduled;

        AddDomainEvent(new TwoHandControlTestScheduledEvent(
            TwoHandControlsEventId.New(), TwoHandControlsId, dueAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: RecordTestResult -----------------------------------------------

    /// <summary>Records the outcome of a scheduled synchronization test.</summary>
    public void RecordTestResult(bool synchronized, decimal synchronizationScore, UserId testedByUserId, UtcTimestamp testedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(TwoHandControlsStatus.TestScheduled);

        if (synchronizationScore is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(synchronizationScore), "Synchronization score must be between 0 and 100.");

        TestedByUserId       = testedByUserId;
        TestedAtUtc           = testedAtUtc;
        SynchronizationScore  = synchronizationScore;

        if (synchronized)
        {
            Status = TwoHandControlsStatus.Synchronized;
            AddDomainEvent(new TwoHandControlSynchronizedEvent(
                TwoHandControlsEventId.New(), TwoHandControlsId, synchronizationScore, testedAtUtc, actor, correlationId, causationId));
        }
        else
        {
            BypassAttemptCount++;
            Status = TwoHandControlsStatus.BypassDetected;
            AddDomainEvent(new TwoHandControlBypassDetectedEvent(
                TwoHandControlsEventId.New(), TwoHandControlsId, BypassAttemptCount, testedAtUtc, actor, correlationId, causationId));
        }
    }

    // ---- Behavior: Retire -----------------------------------------------------------

    /// <summary>Retires this control.</summary>
    public void Retire(UtcTimestamp retiredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status == TwoHandControlsStatus.Retired)
            throw new TwoHandControlsInvalidStatusTransitionException(TwoHandControlsId, Status.ToString(), TwoHandControlsStatus.Retired.ToString(), correlationId, causationId);

        Status = TwoHandControlsStatus.Retired;

        AddDomainEvent(new TwoHandControlRetiredEvent(
            TwoHandControlsEventId.New(), TwoHandControlsId, retiredAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(ITwoHandControlsDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params TwoHandControlsStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for TwoHandControls {TwoHandControlsId}.");
    }
}
