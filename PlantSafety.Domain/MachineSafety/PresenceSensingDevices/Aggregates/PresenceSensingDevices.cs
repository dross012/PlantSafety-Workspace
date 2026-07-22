// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.PresenceSensingDevices
//  Layer:          Domain
//  File:           PresenceSensingDevices.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Aggregate root for a single presence-sensing device: light curtains, safety laser
//                  scanners, or pressure-sensitive mats. Models the real calibration lifecycle:
//                  installation, scheduled calibration, calibrated/out-of-tolerance outcome, trigger
//                  event tracking, and retirement.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.PresenceSensingDevices.Aggregates;

/// <summary>Aggregate root for a single presence-sensing device.</summary>
public sealed partial class PresenceSensingDevices
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this PresenceSensingDevices aggregate.</summary>
    public PresenceSensingDevicesId PresenceSensingDevicesId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public PresenceSensingDevicesStatus Status { get; private set; }

    // ---- Device identity ---------------------------------------------------------

    /// <summary>Human-readable title of this device.</summary>
    public string DeviceTitle { get; private set; } = string.Empty;

    /// <summary>Presence-sensing device type.</summary>
    public PresenceSensingDeviceType DeviceType { get; private set; }

    /// <summary>Location of the protected equipment.</summary>
    public string EquipmentLocation { get; private set; } = string.Empty;

    /// <summary>Identity of the employee who last calibrated this device.</summary>
    public UserId CalibratedByUserId { get; private set; }

    /// <summary>UTC timestamp of the most recent calibration, if any.</summary>
    public UtcTimestamp? CalibratedAtUtc { get; private set; }

    /// <summary>UTC timestamp the next calibration is due.</summary>
    public UtcTimestamp? NextCalibrationDueAtUtc { get; private set; }

    /// <summary>Cumulative count of protective-field trigger events recorded.</summary>
    public int TriggerEventCount { get; private set; }

    /// <summary>Detection accuracy score (0-100) from the most recent calibration.</summary>
    public decimal? DetectionAccuracyScore { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IPresenceSensingDevicesDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IPresenceSensingDevicesDomainEvent> DomainEvents
        => new ReadOnlyCollection<IPresenceSensingDevicesDomainEvent>(_domainEvents);

    // For rehydration only.
    private PresenceSensingDevices() { }

    // ---- Factory: Create (Install) -----------------------------------------------------------

    /// <summary>Records installation of a new presence-sensing device and raises <see cref="PresenceSensingDeviceInstalledEvent"/>.</summary>
    public static PresenceSensingDevices Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string deviceTitle = "",
        PresenceSensingDeviceType deviceType = PresenceSensingDeviceType.LightCurtain,
        string equipmentLocation = "")
    {
        var agg = new PresenceSensingDevices
        {
            PresenceSensingDevicesId = PresenceSensingDevicesId.New(),
            TenantId          = tenantId,
            SiteId            = siteId,
            Status            = PresenceSensingDevicesStatus.Installed,
            DeviceTitle       = (deviceTitle ?? string.Empty).Trim(),
            DeviceType        = deviceType,
            EquipmentLocation = (equipmentLocation ?? string.Empty).Trim(),
            CreatedAtUtc      = createdAtUtc,
            CreatedBy         = createdBy,
        };

        agg.AddDomainEvent(new PresenceSensingDeviceInstalledEvent(
            PresenceSensingDevicesEventId.New(), agg.PresenceSensingDevicesId, deviceType, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: ScheduleCalibration -----------------------------------------------

    /// <summary>Schedules the next calibration.</summary>
    public void ScheduleCalibration(UtcTimestamp dueAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(PresenceSensingDevicesStatus.Installed, PresenceSensingDevicesStatus.Calibrated, PresenceSensingDevicesStatus.OutOfTolerance);

        NextCalibrationDueAtUtc = dueAtUtc;
        Status = PresenceSensingDevicesStatus.CalibrationScheduled;

        AddDomainEvent(new PresenceSensingDeviceCalibrationScheduledEvent(
            PresenceSensingDevicesEventId.New(), PresenceSensingDevicesId, dueAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: RecordCalibrationResult -----------------------------------------------

    /// <summary>Records the outcome of a scheduled calibration.</summary>
    public void RecordCalibrationResult(decimal detectionAccuracyScore, UserId calibratedByUserId, UtcTimestamp calibratedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(PresenceSensingDevicesStatus.CalibrationScheduled);

        if (detectionAccuracyScore is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(detectionAccuracyScore), "Detection accuracy score must be between 0 and 100.");

        CalibratedByUserId      = calibratedByUserId;
        CalibratedAtUtc         = calibratedAtUtc;
        DetectionAccuracyScore  = detectionAccuracyScore;

        if (detectionAccuracyScore >= 95m)
        {
            Status = PresenceSensingDevicesStatus.Calibrated;
            AddDomainEvent(new PresenceSensingDeviceCalibratedEvent(
                PresenceSensingDevicesEventId.New(), PresenceSensingDevicesId, detectionAccuracyScore, calibratedAtUtc, actor, correlationId, causationId));
        }
        else
        {
            Status = PresenceSensingDevicesStatus.OutOfTolerance;
            AddDomainEvent(new PresenceSensingDeviceOutOfToleranceEvent(
                PresenceSensingDevicesEventId.New(), PresenceSensingDevicesId, detectionAccuracyScore, calibratedAtUtc, actor, correlationId, causationId));
        }
    }

    // ---- Behavior: RecordTriggerEvent -----------------------------------------------

    /// <summary>Records that the device's protective field was triggered, stopping the machine.</summary>
    public void RecordTriggerEvent(UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        TriggerEventCount++;

        AddDomainEvent(new PresenceSensingDeviceTriggeredEvent(
            PresenceSensingDevicesEventId.New(), PresenceSensingDevicesId, TriggerEventCount, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Retire -----------------------------------------------------------

    /// <summary>Retires this device.</summary>
    public void Retire(UtcTimestamp retiredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status == PresenceSensingDevicesStatus.Retired)
            throw new PresenceSensingDevicesInvalidStatusTransitionException(PresenceSensingDevicesId, Status.ToString(), PresenceSensingDevicesStatus.Retired.ToString(), correlationId, causationId);

        Status = PresenceSensingDevicesStatus.Retired;

        AddDomainEvent(new PresenceSensingDeviceRetiredEvent(
            PresenceSensingDevicesEventId.New(), PresenceSensingDevicesId, retiredAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IPresenceSensingDevicesDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params PresenceSensingDevicesStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for PresenceSensingDevices {PresenceSensingDevicesId}.");
    }
}
