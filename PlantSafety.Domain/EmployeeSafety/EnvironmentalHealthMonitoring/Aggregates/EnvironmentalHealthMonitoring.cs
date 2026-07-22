// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.EnvironmentalHealthMonitoring
//  Layer:          Domain
//  File:           EnvironmentalHealthMonitoring.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Aggregate root for a single environmental/health monitoring measurement cycle
//                  (air quality, noise, vibration, or respiratory exposure). Protects employees from
//                  airborne, noise, and vibration hazards. Models the real lifecycle: scheduling,
//                  measurement, exceedance detection, corrective action, and resolution.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.EnvironmentalHealthMonitoring.Aggregates;

/// <summary>Aggregate root for a single environmental/health monitoring measurement cycle.</summary>
public sealed partial class EnvironmentalHealthMonitoring
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this EnvironmentalHealthMonitoring aggregate.</summary>
    public EnvironmentalHealthMonitoringId EnvironmentalHealthMonitoringId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public EnvironmentalHealthMonitoringStatus Status { get; private set; }

    // ---- Monitoring identity -----------------------------------------------

    /// <summary>Human-readable title of this monitoring cycle.</summary>
    public string MonitoringTitle { get; private set; } = string.Empty;

    /// <summary>Monitoring category.</summary>
    public EnvironmentalMonitoringCategory Category { get; private set; }

    /// <summary>Plant location being monitored.</summary>
    public string MonitoringLocation { get; private set; } = string.Empty;

    /// <summary>Identity of the employee/technician who performed the measurement.</summary>
    public UserId MonitoredByUserId { get; private set; }

    /// <summary>UTC timestamp the measurement was recorded, if any.</summary>
    public UtcTimestamp? MeasuredAtUtc { get; private set; }

    /// <summary>UTC timestamp the next measurement is due.</summary>
    public UtcTimestamp? NextMeasurementDueAtUtc { get; private set; }

    /// <summary>Exposure level score (0-100, relative to the applicable exposure limit).</summary>
    public decimal? ExposureLevelScore { get; private set; }

    /// <summary>Threshold above which an exceedance is declared.</summary>
    public decimal ExceedanceThreshold { get; private set; } = 100m;

    /// <summary>Count of consecutive exceedances recorded for this monitoring point.</summary>
    public int ExceedanceCount { get; private set; }

    /// <summary>Narrative description of corrective action taken, if any.</summary>
    public string? CorrectiveActionDescription { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IEnvironmentalHealthMonitoringDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IEnvironmentalHealthMonitoringDomainEvent> DomainEvents
        => new ReadOnlyCollection<IEnvironmentalHealthMonitoringDomainEvent>(_domainEvents);

    // For rehydration only.
    private EnvironmentalHealthMonitoring() { }

    // ---- Factory: Create -----------------------------------------------------------

    /// <summary>Schedules a new environmental/health monitoring cycle and raises <see cref="EnvironmentalMonitoringScheduledEvent"/>.</summary>
    public static EnvironmentalHealthMonitoring Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string monitoringTitle = "",
        EnvironmentalMonitoringCategory category = EnvironmentalMonitoringCategory.AirQuality,
        string monitoringLocation = "",
        UserId monitoredByUserId = default,
        decimal exceedanceThreshold = 100m)
    {
        var agg = new EnvironmentalHealthMonitoring
        {
            EnvironmentalHealthMonitoringId = EnvironmentalHealthMonitoringId.New(),
            TenantId              = tenantId,
            SiteId                = siteId,
            Status                = EnvironmentalHealthMonitoringStatus.Scheduled,
            MonitoringTitle       = (monitoringTitle ?? string.Empty).Trim(),
            Category              = category,
            MonitoringLocation    = (monitoringLocation ?? string.Empty).Trim(),
            MonitoredByUserId     = monitoredByUserId,
            ExceedanceThreshold   = exceedanceThreshold,
            CreatedAtUtc          = createdAtUtc,
            CreatedBy             = createdBy,
        };

        agg.AddDomainEvent(new EnvironmentalMonitoringScheduledEvent(
            EnvironmentalHealthMonitoringEventId.New(), agg.EnvironmentalHealthMonitoringId, category,
            createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: RecordMeasurement ---------------------------------------------

    /// <summary>Records a measurement and determines whether it exceeds the applicable exposure limit.</summary>
    public void RecordMeasurement(decimal exposureLevelScore, UtcTimestamp measuredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(EnvironmentalHealthMonitoringStatus.Scheduled, EnvironmentalHealthMonitoringStatus.WithinLimits, EnvironmentalHealthMonitoringStatus.Resolved);

        ExposureLevelScore = exposureLevelScore;
        MeasuredAtUtc       = measuredAtUtc;

        if (exposureLevelScore > ExceedanceThreshold)
        {
            ExceedanceCount++;
            Status = EnvironmentalHealthMonitoringStatus.ExceedanceDetected;
            AddDomainEvent(new EnvironmentalMonitoringExceedanceDetectedEvent(
                EnvironmentalHealthMonitoringEventId.New(), EnvironmentalHealthMonitoringId, exposureLevelScore, ExceedanceCount,
                measuredAtUtc, actor, correlationId, causationId));
        }
        else
        {
            Status = EnvironmentalHealthMonitoringStatus.WithinLimits;
            AddDomainEvent(new EnvironmentalMonitoringMeasurementRecordedEvent(
                EnvironmentalHealthMonitoringEventId.New(), EnvironmentalHealthMonitoringId, exposureLevelScore,
                measuredAtUtc, actor, correlationId, causationId));
        }
    }

    // ---- Behavior: BeginCorrectiveAction ---------------------------------------------

    /// <summary>Begins corrective action to remediate an exceedance.</summary>
    public void BeginCorrectiveAction(string description, UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(EnvironmentalHealthMonitoringStatus.ExceedanceDetected);

        CorrectiveActionDescription = description;
        Status = EnvironmentalHealthMonitoringStatus.CorrectiveActionInProgress;

        AddDomainEvent(new EnvironmentalMonitoringCorrectiveActionStartedEvent(
            EnvironmentalHealthMonitoringEventId.New(), EnvironmentalHealthMonitoringId, description,
            occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: ResolveExceedance ---------------------------------------------

    /// <summary>Resolves the exceedance once re-measurement confirms compliance.</summary>
    public void ResolveExceedance(decimal reMeasuredScore, UtcTimestamp resolvedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(EnvironmentalHealthMonitoringStatus.CorrectiveActionInProgress);

        ExposureLevelScore = reMeasuredScore;
        MeasuredAtUtc       = resolvedAtUtc;
        Status = EnvironmentalHealthMonitoringStatus.Resolved;

        AddDomainEvent(new EnvironmentalMonitoringExceedanceResolvedEvent(
            EnvironmentalHealthMonitoringEventId.New(), EnvironmentalHealthMonitoringId, reMeasuredScore,
            resolvedAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IEnvironmentalHealthMonitoringDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params EnvironmentalHealthMonitoringStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for EnvironmentalHealthMonitoring {EnvironmentalHealthMonitoringId}.");
    }
}
