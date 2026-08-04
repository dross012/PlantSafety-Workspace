// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Domain
//  File:           AdvancedMetricsLedger.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for a single tenant/site's granular, ESG-grade metrics ledger for
//                  one reporting period: Scope 1/2/3 carbon accounting, energy/water intensity,
//                  waste diversion, and sustainability target tracking. Sits alongside (not inside)
//                  the pre-existing generic Sustainability aggregate -- this aggregate is the
//                  auditable, methodology-versioned metrics substrate that
//                  Sustainability.AdvancedMetrics.EsgDisclosureReport is generated from.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Sustainability.AdvancedMetrics.Aggregates;

/// <summary>Aggregate root for a tenant/site's ESG-grade advanced metrics ledger for one reporting period.</summary>
public sealed class AdvancedMetricsLedger
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this ledger aggregate.</summary>
    public AdvancedMetricsLedgerId AdvancedMetricsLedgerId { get; private set; }

    /// <summary>Tenant that owns this ledger.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this ledger is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Start of the reporting period this ledger covers.</summary>
    public UtcTimestamp ReportingPeriodStartUtc { get; private set; }

    /// <summary>End of the reporting period this ledger covers.</summary>
    public UtcTimestamp ReportingPeriodEndUtc { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public AdvancedMetricsLedgerStatus Status { get; private set; }

    // ---- Child records ---------------------------------------------------------

    private readonly List<CarbonEmissionRecord> _carbonEmissionRecords = [];
    private readonly List<EnergyIntensityMetric> _energyIntensityMetrics = [];
    private readonly List<WaterIntensityMetric> _waterIntensityMetrics = [];
    private readonly List<WasteDiversionRecord> _wasteDiversionRecords = [];
    private readonly List<SustainabilityTarget> _sustainabilityTargets = [];

    /// <summary>Scope 1/2/3 carbon emission records captured in this reporting period.</summary>
    public IReadOnlyCollection<CarbonEmissionRecord> CarbonEmissionRecords => _carbonEmissionRecords.AsReadOnly();

    /// <summary>Energy intensity meter-read records captured in this reporting period.</summary>
    public IReadOnlyCollection<EnergyIntensityMetric> EnergyIntensityMetrics => _energyIntensityMetrics.AsReadOnly();

    /// <summary>Water intensity meter-read records captured in this reporting period.</summary>
    public IReadOnlyCollection<WaterIntensityMetric> WaterIntensityMetrics => _waterIntensityMetrics.AsReadOnly();

    /// <summary>Waste diversion records captured in this reporting period.</summary>
    public IReadOnlyCollection<WasteDiversionRecord> WasteDiversionRecords => _wasteDiversionRecords.AsReadOnly();

    /// <summary>Sustainability KPI targets tracked against this ledger.</summary>
    public IReadOnlyCollection<SustainabilityTarget> SustainabilityTargets => _sustainabilityTargets.AsReadOnly();

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IAdvancedMetricsDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IAdvancedMetricsDomainEvent> DomainEvents
        => new ReadOnlyCollection<IAdvancedMetricsDomainEvent>(_domainEvents);

    // For rehydration only.
    private AdvancedMetricsLedger() { }

    // ---- Factory: Create (Open) -----------------------------------------------------------

    /// <summary>Opens a new AdvancedMetricsLedger for the given tenant/site/reporting period.</summary>
    public static AdvancedMetricsLedger Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        UtcTimestamp reportingPeriodStartUtc,
        UtcTimestamp reportingPeriodEndUtc)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("TenantId must not be empty.", nameof(tenantId));
        if (reportingPeriodEndUtc.Value <= reportingPeriodStartUtc.Value)
            throw new ArgumentException("ReportingPeriodEndUtc must be after ReportingPeriodStartUtc.", nameof(reportingPeriodEndUtc));

        return new AdvancedMetricsLedger
        {
            AdvancedMetricsLedgerId = AdvancedMetricsLedgerId.New(),
            TenantId                 = tenantId,
            SiteId                   = siteId,
            ReportingPeriodStartUtc  = reportingPeriodStartUtc,
            ReportingPeriodEndUtc    = reportingPeriodEndUtc,
            Status                   = AdvancedMetricsLedgerStatus.Open,
            CreatedAtUtc             = createdAtUtc,
            CreatedBy                = createdBy,
        };
        // Deliberately no "Ledger opened" event: the ledger itself is an implicit accounting
        // substrate; the first recorded metric is the first observable domain event, mirroring
        // how a period's books are opened silently and only entries are audit-events.
    }

    // ---- Behavior: RecordCarbonEmission -----------------------------------------------

    /// <summary>Records a Scope 1/2/3 carbon emission activity.</summary>
    public CarbonEmissionRecord RecordCarbonEmission(
        GhgScope scope,
        string activityDescription,
        decimal emissionsMetricTonsCo2e,
        string methodologyVersion,
        UtcTimestamp activityAtUtc,
        UtcTimestamp recordedAtUtc,
        UserId recordedBy,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureCanRecordMetrics(correlationId, causationId);

        var record = new CarbonEmissionRecord(
            CarbonEmissionRecordId.New(), scope, activityDescription, emissionsMetricTonsCo2e,
            methodologyVersion, activityAtUtc, recordedAtUtc, recordedBy);

        _carbonEmissionRecords.Add(record);

        AddDomainEvent(new CarbonEmissionRecordedEvent(
            AdvancedMetricsEventId.New(), AdvancedMetricsLedgerId, record.CarbonEmissionRecordId,
            scope, emissionsMetricTonsCo2e, methodologyVersion.Trim(), recordedAtUtc, actor, correlationId, causationId));

        return record;
    }

    // ---- Behavior: RecordEnergyIntensityMetric -----------------------------------------------

    /// <summary>Records an energy-intensity meter read.</summary>
    public EnergyIntensityMetric RecordEnergyIntensityMetric(
        decimal energyConsumedKwh,
        decimal outputUnits,
        string methodologyVersion,
        UtcTimestamp readingAtUtc,
        UtcTimestamp recordedAtUtc,
        UserId recordedBy,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureCanRecordMetrics(correlationId, causationId);

        var metric = new EnergyIntensityMetric(
            EnergyIntensityMetricId.New(), energyConsumedKwh, outputUnits, methodologyVersion,
            readingAtUtc, recordedAtUtc, recordedBy);

        _energyIntensityMetrics.Add(metric);

        AddDomainEvent(new EnergyIntensityMetricRecordedEvent(
            AdvancedMetricsEventId.New(), AdvancedMetricsLedgerId, metric.EnergyIntensityMetricId,
            metric.IntensityValue, methodologyVersion.Trim(), recordedAtUtc, actor, correlationId, causationId));

        return metric;
    }

    // ---- Behavior: RecordWaterIntensityMetric -----------------------------------------------

    /// <summary>Records a water-intensity meter read.</summary>
    public WaterIntensityMetric RecordWaterIntensityMetric(
        decimal waterConsumedCubicMeters,
        decimal outputUnits,
        string methodologyVersion,
        UtcTimestamp readingAtUtc,
        UtcTimestamp recordedAtUtc,
        UserId recordedBy,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureCanRecordMetrics(correlationId, causationId);

        var metric = new WaterIntensityMetric(
            WaterIntensityMetricId.New(), waterConsumedCubicMeters, outputUnits, methodologyVersion,
            readingAtUtc, recordedAtUtc, recordedBy);

        _waterIntensityMetrics.Add(metric);

        AddDomainEvent(new WaterIntensityMetricRecordedEvent(
            AdvancedMetricsEventId.New(), AdvancedMetricsLedgerId, metric.WaterIntensityMetricId,
            metric.IntensityValue, methodologyVersion.Trim(), recordedAtUtc, actor, correlationId, causationId));

        return metric;
    }

    // ---- Behavior: RecordWasteDiversion -----------------------------------------------

    /// <summary>Records a waste diversion interval.</summary>
    public WasteDiversionRecord RecordWasteDiversion(
        decimal totalWasteKg,
        decimal divertedWasteKg,
        string methodologyVersion,
        UtcTimestamp intervalEndAtUtc,
        UtcTimestamp recordedAtUtc,
        UserId recordedBy,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureCanRecordMetrics(correlationId, causationId);

        var record = new WasteDiversionRecord(
            WasteDiversionRecordId.New(), totalWasteKg, divertedWasteKg, methodologyVersion,
            intervalEndAtUtc, recordedAtUtc, recordedBy);

        _wasteDiversionRecords.Add(record);

        AddDomainEvent(new WasteDiversionRecordedEvent(
            AdvancedMetricsEventId.New(), AdvancedMetricsLedgerId, record.WasteDiversionRecordId,
            record.DiversionRatePercent, methodologyVersion.Trim(), recordedAtUtc, actor, correlationId, causationId));

        return record;
    }

    // ---- Behavior: SetSustainabilityTarget -----------------------------------------------

    /// <summary>Sets (or re-baselines) a sustainability KPI target on this ledger.</summary>
    public SustainabilityTarget SetSustainabilityTarget(
        SustainabilityTargetType targetType,
        decimal baselineValue,
        decimal targetValue,
        UtcTimestamp targetDateUtc,
        UtcTimestamp setAtUtc,
        UserId setBy,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureCanRecordMetrics(correlationId, causationId);

        var target = new SustainabilityTarget(
            SustainabilityTargetId.New(), targetType, baselineValue, targetValue, targetDateUtc, setAtUtc, setBy);

        _sustainabilityTargets.Add(target);

        AddDomainEvent(new SustainabilityTargetSetEvent(
            AdvancedMetricsEventId.New(), AdvancedMetricsLedgerId, target.SustainabilityTargetId,
            targetType, targetValue, targetDateUtc, setAtUtc, actor, correlationId, causationId));

        return target;
    }

    // ---- Behavior: UpdateTargetProgress -----------------------------------------------

    /// <summary>
    /// Updates a tracked target's current value, re-evaluates its status via
    /// <see cref="SustainabilityTargetRiskPolicy"/>, and raises <see cref="SustainabilityTargetAtRiskEvent"/>
    /// the moment a target newly trends behind plan (risk hook for RiskManagement).
    /// </summary>
    public void UpdateTargetProgress(
        SustainabilityTargetId sustainabilityTargetId,
        decimal currentValue,
        UtcTimestamp atUtc,
        bool higherIsBetter,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        var target = _sustainabilityTargets.FirstOrDefault(t => t.SustainabilityTargetId.Equals(sustainabilityTargetId))
            ?? throw new ArgumentException($"SustainabilityTarget {sustainabilityTargetId} not found on ledger {AdvancedMetricsLedgerId}.", nameof(sustainabilityTargetId));

        var newStatus = SustainabilityTargetRiskPolicy.Evaluate(target, currentValue, atUtc, higherIsBetter);
        var becameAtRisk = target.UpdateProgress(currentValue, newStatus, atUtc);

        if (becameAtRisk)
        {
            AddDomainEvent(new SustainabilityTargetAtRiskEvent(
                AdvancedMetricsEventId.New(), AdvancedMetricsLedgerId, target.SustainabilityTargetId,
                target.TargetType, currentValue, target.TargetValue, atUtc, actor, correlationId, causationId));
        }
    }

    // ---- Behavior: Close -----------------------------------------------------------

    /// <summary>Closes the ledger at the end of its reporting period, stopping further record intake.</summary>
    public void Close(UtcTimestamp atUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (!AdvancedMetricsLedgerLifecycleRules.CanClose(Status))
            throw new AdvancedMetricsInvalidStatusTransitionException(
                AdvancedMetricsLedgerId, Status.ToString(), AdvancedMetricsLedgerStatus.Closed.ToString(), correlationId, causationId);

        Status = AdvancedMetricsLedgerStatus.Closed;
    }

    // ---- Behavior: Archive -----------------------------------------------------------

    /// <summary>Archives a closed ledger for long-term compliance retention.</summary>
    public void Archive(UtcTimestamp atUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (!AdvancedMetricsLedgerLifecycleRules.CanArchive(Status))
            throw new AdvancedMetricsInvalidStatusTransitionException(
                AdvancedMetricsLedgerId, Status.ToString(), AdvancedMetricsLedgerStatus.Archived.ToString(), correlationId, causationId);

        Status = AdvancedMetricsLedgerStatus.Archived;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IAdvancedMetricsDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureCanRecordMetrics(CorrelationId correlationId, CausationId causationId)
    {
        if (!AdvancedMetricsLedgerLifecycleRules.CanRecordMetrics(Status))
            throw new AdvancedMetricsInvalidStatusTransitionException(
                AdvancedMetricsLedgerId, Status.ToString(), AdvancedMetricsLedgerStatus.Open.ToString(), correlationId, causationId);
    }
}
