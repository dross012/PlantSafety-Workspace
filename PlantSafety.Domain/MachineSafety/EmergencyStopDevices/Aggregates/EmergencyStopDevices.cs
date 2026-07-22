// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.EmergencyStopDevices
//  Layer:          Domain
//  File:           EmergencyStopDevices.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Aggregate root for a single Emergency Stop (E-Stop) device: push-button stops,
//                  pull-cord systems, or cable-actuated stops. Models the real reliability-test
//                  lifecycle: installation, scheduled testing, operational/failure outcome, repair, and
//                  retirement.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.EmergencyStopDevices.Aggregates;

/// <summary>Aggregate root for a single Emergency Stop (E-Stop) device.</summary>
public sealed partial class EmergencyStopDevices
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this EmergencyStopDevices aggregate.</summary>
    public EmergencyStopDevicesId EmergencyStopDevicesId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public EmergencyStopDevicesStatus Status { get; private set; }

    // ---- Device identity ---------------------------------------------------------

    /// <summary>Human-readable title of this device.</summary>
    public string DeviceTitle { get; private set; } = string.Empty;

    /// <summary>E-Stop device type.</summary>
    public EStopDeviceType DeviceType { get; private set; }

    /// <summary>Location of the protected equipment.</summary>
    public string EquipmentLocation { get; private set; } = string.Empty;

    /// <summary>Identity of the employee who last tested this device.</summary>
    public UserId TestedByUserId { get; private set; }

    /// <summary>UTC timestamp of the most recent test, if any.</summary>
    public UtcTimestamp? TestedAtUtc { get; private set; }

    /// <summary>UTC timestamp the next test is due.</summary>
    public UtcTimestamp? NextTestDueAtUtc { get; private set; }

    /// <summary>Cumulative count of test failures recorded.</summary>
    public int FailureCount { get; private set; }

    /// <summary>Response time score (0-100) from the most recent test.</summary>
    public decimal? ResponseTimeScore { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IEmergencyStopDevicesDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IEmergencyStopDevicesDomainEvent> DomainEvents
        => new ReadOnlyCollection<IEmergencyStopDevicesDomainEvent>(_domainEvents);

    // For rehydration only.
    private EmergencyStopDevices() { }

    // ---- Factory: Create (Install) -----------------------------------------------------------

    /// <summary>Records installation of a new E-Stop device and raises <see cref="EmergencyStopDeviceInstalledEvent"/>.</summary>
    public static EmergencyStopDevices Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string deviceTitle = "",
        EStopDeviceType deviceType = EStopDeviceType.PushButtonStop,
        string equipmentLocation = "")
    {
        var agg = new EmergencyStopDevices
        {
            EmergencyStopDevicesId = EmergencyStopDevicesId.New(),
            TenantId          = tenantId,
            SiteId            = siteId,
            Status            = EmergencyStopDevicesStatus.Installed,
            DeviceTitle       = (deviceTitle ?? string.Empty).Trim(),
            DeviceType        = deviceType,
            EquipmentLocation = (equipmentLocation ?? string.Empty).Trim(),
            CreatedAtUtc      = createdAtUtc,
            CreatedBy         = createdBy,
        };

        agg.AddDomainEvent(new EmergencyStopDeviceInstalledEvent(
            EmergencyStopDevicesEventId.New(), agg.EmergencyStopDevicesId, deviceType, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: ScheduleTest -----------------------------------------------

    /// <summary>Schedules the next functional (reliability) test.</summary>
    public void ScheduleTest(UtcTimestamp dueAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(EmergencyStopDevicesStatus.Installed, EmergencyStopDevicesStatus.Operational);

        NextTestDueAtUtc = dueAtUtc;
        Status = EmergencyStopDevicesStatus.TestScheduled;

        AddDomainEvent(new EmergencyStopDeviceTestScheduledEvent(
            EmergencyStopDevicesEventId.New(), EmergencyStopDevicesId, dueAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: RecordTestResult -----------------------------------------------

    /// <summary>Records the outcome of a scheduled functional test.</summary>
    public void RecordTestResult(bool passed, decimal responseTimeScore, UserId testedByUserId, UtcTimestamp testedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(EmergencyStopDevicesStatus.TestScheduled);

        if (responseTimeScore is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(responseTimeScore), "Response time score must be between 0 and 100.");

        TestedByUserId    = testedByUserId;
        TestedAtUtc        = testedAtUtc;
        ResponseTimeScore  = responseTimeScore;

        if (passed)
        {
            Status = EmergencyStopDevicesStatus.Operational;
            AddDomainEvent(new EmergencyStopDeviceTestPassedEvent(
                EmergencyStopDevicesEventId.New(), EmergencyStopDevicesId, responseTimeScore, testedAtUtc, actor, correlationId, causationId));
        }
        else
        {
            FailureCount++;
            Status = EmergencyStopDevicesStatus.FailureDetected;
            AddDomainEvent(new EmergencyStopDeviceFailureDetectedEvent(
                EmergencyStopDevicesEventId.New(), EmergencyStopDevicesId, FailureCount, testedAtUtc, actor, correlationId, causationId));
        }
    }

    // ---- Behavior: BeginRepair -----------------------------------------------

    /// <summary>Begins repair after a test failure.</summary>
    public void BeginRepair(UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(EmergencyStopDevicesStatus.FailureDetected);

        Status = EmergencyStopDevicesStatus.RepairInProgress;

        AddDomainEvent(new EmergencyStopDeviceRepairStartedEvent(
            EmergencyStopDevicesEventId.New(), EmergencyStopDevicesId, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Retire -----------------------------------------------------------

    /// <summary>Retires the device.</summary>
    public void Retire(UtcTimestamp retiredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status == EmergencyStopDevicesStatus.Retired)
            throw new EmergencyStopDevicesInvalidStatusTransitionException(EmergencyStopDevicesId, Status.ToString(), EmergencyStopDevicesStatus.Retired.ToString(), correlationId, causationId);

        Status = EmergencyStopDevicesStatus.Retired;

        AddDomainEvent(new EmergencyStopDeviceRetiredEvent(
            EmergencyStopDevicesEventId.New(), EmergencyStopDevicesId, retiredAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IEmergencyStopDevicesDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params EmergencyStopDevicesStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for EmergencyStopDevices {EmergencyStopDevicesId}.");
    }
}
