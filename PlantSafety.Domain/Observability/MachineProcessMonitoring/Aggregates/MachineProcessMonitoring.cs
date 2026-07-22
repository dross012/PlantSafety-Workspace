// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.MachineProcessMonitoring
//  Layer:          Domain
//  File:           MachineProcessMonitoring.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the MachineProcessMonitoring bounded context (module: Observability).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.MachineProcessMonitoring.Aggregates;

/// <summary>Aggregate root for the MachineProcessMonitoring bounded context.</summary>
public sealed partial class MachineProcessMonitoring
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this MachineProcessMonitoring aggregate.</summary>
    public MachineProcessMonitoringId MachineProcessMonitoringId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public MachineProcessMonitoringStatus Status { get; private set; }

    // ---- MachineProcessMonitoring-specific business properties --------------------------------

    /// <summary>MachineId business attribute.</summary>
    public Guid MachineId { get; private set; }

    /// <summary>SensorType business attribute.</summary>
    public string SensorType { get; private set; }

    /// <summary>Reading business attribute.</summary>
    public decimal Reading { get; private set; }

    /// <summary>ThresholdBreached business attribute.</summary>
    public bool ThresholdBreached { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IMachineProcessMonitoringDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IMachineProcessMonitoringDomainEvent> DomainEvents
        => new ReadOnlyCollection<IMachineProcessMonitoringDomainEvent>(_domainEvents);

    // For rehydration only.
    private MachineProcessMonitoring() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new MachineProcessMonitoring aggregate and raises <see cref="MachineProcessMonitoringCreatedEvent"/>.</summary>
    public static MachineProcessMonitoring Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        Guid machineId = default,
        string sensorType = "",
        decimal reading = 0m,
        bool thresholdBreached = false)
    {
        var agg = new MachineProcessMonitoring
        {
            MachineProcessMonitoringId     = MachineProcessMonitoringId.New(),
            TenantId      = tenantId,
            SiteId        = siteId,
            Status        = MachineProcessMonitoringStatus.Normal,
            MachineId = machineId,
            SensorType = (sensorType ?? string.Empty).Trim(),
            Reading = reading,
            ThresholdBreached = thresholdBreached,
            CreatedAtUtc  = createdAtUtc,
            CreatedBy     = createdBy,
        };
        if (thresholdBreached)
            agg.Status = MachineProcessMonitoringStatus.ThresholdBreached;

        agg.AddDomainEvent(new MachineProcessMonitoringCreatedEvent(
            EventId:       new MachineProcessMonitoringEventId(Uuid7.New()),
            MachineProcessMonitoringId:      agg.MachineProcessMonitoringId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    /// <summary>Identity of the user who acknowledged the most recent breach/anomaly, if any.</summary>
    public UserId? AcknowledgedBy { get; private set; }

    /// <summary>Free-text description recorded for the most recent anomaly flag.</summary>
    public string? AnomalyDescription { get; private set; }

    // ---- Lifecycle: Machine & Process Safety Monitoring ----------------------

    /// <summary>
    /// Records a new sensor reading ("Real-time hazard alerts"). Automatically transitions to
    /// <see cref="MachineProcessMonitoringStatus.ThresholdBreached"/> when the reading falls
    /// outside [<paramref name="thresholdMin"/>, <paramref name="thresholdMax"/>].
    /// Valid from <see cref="MachineProcessMonitoringStatus.Normal"/> or <see cref="MachineProcessMonitoringStatus.ThresholdBreached"/>.
    /// </summary>
    public void RecordSensorReading(decimal reading, decimal thresholdMin, decimal thresholdMax, UtcTimestamp recordedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, MachineProcessMonitoringStatus.Normal, MachineProcessMonitoringStatus.ThresholdBreached);

        Reading = reading;
        ThresholdBreached = reading < thresholdMin || reading > thresholdMax;
        Status = ThresholdBreached ? MachineProcessMonitoringStatus.ThresholdBreached : MachineProcessMonitoringStatus.Normal;

        AddDomainEvent(new MachineProcessMonitoringSensorReadingRecordedEvent(
            EventId: new MachineProcessMonitoringEventId(Uuid7.New()),
            MachineProcessMonitoringId: MachineProcessMonitoringId,
            Reading: reading,
            ThresholdBreached: ThresholdBreached,
            RecordedAtUtc: recordedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Raises an AI/ML process anomaly flag (unexpected startup, bypassed guard, disabled
    /// interlock). Valid from <see cref="MachineProcessMonitoringStatus.Normal"/> or
    /// <see cref="MachineProcessMonitoringStatus.ThresholdBreached"/>.
    /// </summary>
    public void RaiseAnomalyFlag(string anomalyDescription, UtcTimestamp flaggedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, MachineProcessMonitoringStatus.Normal, MachineProcessMonitoringStatus.ThresholdBreached);
        if (string.IsNullOrWhiteSpace(anomalyDescription))
            throw new ArgumentException("An anomaly description must be supplied.", nameof(anomalyDescription));

        AnomalyDescription = anomalyDescription.Trim();
        Status = MachineProcessMonitoringStatus.AnomalyFlagged;

        AddDomainEvent(new MachineProcessMonitoringAnomalyFlaggedEvent(
            EventId: new MachineProcessMonitoringEventId(Uuid7.New()),
            MachineProcessMonitoringId: MachineProcessMonitoringId,
            AnomalyDescription: AnomalyDescription,
            FlaggedAtUtc: flaggedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>Acknowledges a threshold breach or anomaly flag. Valid from <see cref="MachineProcessMonitoringStatus.ThresholdBreached"/> or <see cref="MachineProcessMonitoringStatus.AnomalyFlagged"/>.</summary>
    public void Acknowledge(UserId acknowledgedBy, UtcTimestamp acknowledgedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, MachineProcessMonitoringStatus.ThresholdBreached, MachineProcessMonitoringStatus.AnomalyFlagged);

        AcknowledgedBy = acknowledgedBy;
        Status = MachineProcessMonitoringStatus.Acknowledged;

        AddDomainEvent(new MachineProcessMonitoringAcknowledgedEvent(
            EventId: new MachineProcessMonitoringEventId(Uuid7.New()),
            MachineProcessMonitoringId: MachineProcessMonitoringId,
            AcknowledgedBy: acknowledgedBy,
            AcknowledgedAtUtc: acknowledgedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IMachineProcessMonitoringDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params MachineProcessMonitoringStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for MachineProcessMonitoring {MachineProcessMonitoringId}.");
    }

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, params MachineProcessMonitoringStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new MachineProcessMonitoringInvalidStatusTransitionException(
            MachineProcessMonitoringId, Status.ToString(), string.Join("|", allowed.Select(a => a.ToString())),
            correlationId, causationId);
    }
}