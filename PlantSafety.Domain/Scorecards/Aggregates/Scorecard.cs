// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scorecards
//  Layer:          Domain
//  File:           Scorecard.cs
//  Created:        2026-07-20
//  Updated:        2026-07-21 -- replaced the generic single-Create() placeholder with the real
//                  scorecard compilation/distribution lifecycle documented in the Safety
//                  Scorecards Module: KPI measurements are recorded per the 14-metric catalog
//                  (Lagging/Leading/Cultural, see Enums/KpiMetricDefinition.cs) sourced from
//                  "Scorecard Data Sources", then the scorecard is Compiled, Published, and
//                  Distributed before the reporting period Closes. KPI measurements are modeled
//                  as a related child entity (see Entities/KpiMeasurement.cs) because each metric
//                  carries its own numerator/denominator inputs, computed value, and data-source
//                  attribution.
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the Scorecards bounded context. Tracks a single periodic
//                  safety scorecard: its reporting period, compiled KPI measurements, and
//                  publication/distribution history.
//                  Additive-only evolution.
// ============================================================================
using System.Collections.ObjectModel;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Common.Abstractions;
using PlantSafety.Domain.Scorecards.Entities;
using PlantSafety.Domain.Scorecards.Enums;
using PlantSafety.Domain.Scorecards.Events;
using PlantSafety.Domain.Scorecards.Exceptions;
using PlantSafety.Domain.Scorecards.ValueObjects.Ids;

namespace PlantSafety.Domain.Scorecards.Aggregates;

/// <summary>Aggregate root for the Scorecards bounded context.</summary>
public sealed partial class Scorecard
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this Scorecard aggregate.</summary>
    public ScorecardId ScorecardId { get; private set; }

    /// <summary>Tenant that owns this scorecard record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this scorecard is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public ScorecardStatus Status { get; private set; }

    // ---- Scorecard identity & period ------------------------------------------

    /// <summary>Human-readable scorecard title (e.g. "Plant 3 Monthly Safety Scorecard - July 2026").</summary>
    public string ScorecardTitle { get; private set; } = string.Empty;

    /// <summary>Monotonically increasing scorecard revision number, starting at 1.</summary>
    public int ScorecardVersion { get; private set; } = 1;

    /// <summary>Primary indicator category emphasis for this scorecard (Lagging/Leading/Cultural), for panel routing.</summary>
    public IndicatorCategory Category { get; private set; } = IndicatorCategory.Default;

    /// <summary>Reporting period cadence (Monthly/Quarterly/Annual).</summary>
    public ScorecardPeriodType PeriodType { get; private set; } = ScorecardPeriodType.Default;

    /// <summary>UTC instant the reporting period starts (inclusive).</summary>
    public UtcTimestamp PeriodStartUtc { get; private set; }

    /// <summary>UTC instant the reporting period ends (exclusive).</summary>
    public UtcTimestamp PeriodEndUtc { get; private set; }

    /// <summary>Identity of the accountable owner.</summary>
    public UserId OwnerId { get; private set; }

    // ---- KPI measurements ------------------------------------------------------

    private readonly List<KpiMeasurement> _measurements = [];

    /// <summary>KPI measurements recorded for this scorecard's reporting period.</summary>
    public IReadOnlyList<KpiMeasurement> Measurements => _measurements.AsReadOnly();

    // ---- Publication & distribution ----------------------------------------------

    /// <summary>UTC instant the scorecard was published, if any.</summary>
    public UtcTimestamp? PublishedAtUtc { get; private set; }

    /// <summary>Count of distribution events recorded (dashboards, leadership reviews, OSHA inspections, posted areas, meetings).</summary>
    public int DistributionCount { get; private set; }

    /// <summary>Indicates whether formal review/acknowledgment is required for this record.</summary>
    public bool ReviewRequired { get; private set; } = true;

    /// <summary>Count of personnel/stakeholders who have reviewed or acknowledged this record.</summary>
    public int ReviewCount { get; private set; }

    // ---- Audit ---------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IScorecardDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IScorecardDomainEvent> DomainEvents
        => new ReadOnlyCollection<IScorecardDomainEvent>(_domainEvents);

    // For rehydration only.
    private Scorecard() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Opens a new Scorecard reporting period and raises <see cref="ScorecardCreatedEvent"/>.</summary>
    public static Scorecard Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string scorecardTitle = "",
        IndicatorCategory? category = null,
        ScorecardPeriodType? periodType = null,
        UtcTimestamp? periodStartUtc = null,
        UserId ownerId = default,
        bool reviewRequired = true)
    {
        var effectivePeriodType = periodType ?? ScorecardPeriodType.Default;
        var periodStart = periodStartUtc ?? createdAtUtc;

        var agg = new Scorecard
        {
            ScorecardId      = ScorecardId.New(),
            TenantId         = tenantId,
            SiteId           = siteId,
            Status           = ScorecardStatus.Draft,
            ScorecardTitle   = (scorecardTitle ?? string.Empty).Trim(),
            ScorecardVersion = 1,
            Category         = category ?? IndicatorCategory.Default,
            PeriodType       = effectivePeriodType,
            PeriodStartUtc   = periodStart,
            PeriodEndUtc     = UtcTimestamp.From(effectivePeriodType.ComputePeriodEndUtc(periodStart.Value)),
            OwnerId          = ownerId,
            ReviewRequired   = reviewRequired,
            CreatedAtUtc     = createdAtUtc,
            CreatedBy        = createdBy,
        };

        agg.AddDomainEvent(new ScorecardCreatedEvent(
            EventId:       ScorecardEventId.New(),
            ScorecardId:   agg.ScorecardId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Compilation ---------------------------------------------------------

    /// <summary>Records (or overwrites) a KPI measurement sourced from the owning module for this period.</summary>
    public KpiMeasurement RecordMetric(
        KpiMetricDefinition metric, decimal numerator, decimal denominator, string dataSourceModule,
        UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(ScorecardStatus.Draft, ScorecardStatus.Compiled);

        _measurements.RemoveAll(m => m.Metric.Code == metric.Code);
        var measurement = new KpiMeasurement(ScorecardId, metric, numerator, denominator, dataSourceModule, nowUtc);
        _measurements.Add(measurement);

        AddDomainEvent(new ScorecardMetricRecordedEvent(
            ScorecardEventId.New(), ScorecardId, measurement.KpiMeasurementId, metric, measurement.ComputedValue, nowUtc, actor, correlationId, causationId));

        return measurement;
    }

    /// <summary>Marks compilation complete once the required KPI measurements have been recorded.</summary>
    public void CompleteCompilation(UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status != ScorecardStatus.Draft)
            throw new ScorecardInvalidStatusTransitionException(ScorecardId, Status.ToString(), ScorecardStatus.Compiled.ToString(), correlationId, causationId);

        if (_measurements.Count == 0)
            throw new ScorecardNoMeasurementsRecordedException(ScorecardId, correlationId, causationId);

        Status = ScorecardStatus.Compiled;

        AddDomainEvent(new ScorecardCompiledEvent(
            ScorecardEventId.New(), ScorecardId, _measurements.Count, nowUtc, actor, correlationId, causationId));
    }

    // ---- Publication & distribution -------------------------------------------

    /// <summary>Publishes a Compiled scorecard.</summary>
    public void Publish(UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status != ScorecardStatus.Compiled)
            throw new ScorecardInvalidStatusTransitionException(ScorecardId, Status.ToString(), ScorecardStatus.Published.ToString(), correlationId, causationId);

        Status = ScorecardStatus.Published;
        PublishedAtUtc = nowUtc;

        AddDomainEvent(new ScorecardPublishedEvent(
            ScorecardEventId.New(), ScorecardId, nowUtc, actor, correlationId, causationId));
    }

    /// <summary>Records a distribution event for a Published scorecard (dashboard, leadership review, OSHA inspection, posted area, meeting, annual report).</summary>
    public void RecordDistribution(string channel, UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status != ScorecardStatus.Published)
            throw new ScorecardInvalidStatusTransitionException(ScorecardId, Status.ToString(), Status.ToString(), correlationId, causationId);

        DistributionCount++;

        AddDomainEvent(new ScorecardDistributedEvent(
            ScorecardEventId.New(), ScorecardId, (channel ?? string.Empty).Trim(), nowUtc, actor, correlationId, causationId));
    }

    /// <summary>Closes the scorecard once its reporting period has ended.</summary>
    public void Close(UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status != ScorecardStatus.Published)
            throw new ScorecardInvalidStatusTransitionException(ScorecardId, Status.ToString(), ScorecardStatus.Closed.ToString(), correlationId, causationId);

        Status = ScorecardStatus.Closed;

        AddDomainEvent(new ScorecardClosedEvent(
            ScorecardEventId.New(), ScorecardId, nowUtc, actor, correlationId, causationId));
    }

    /// <summary>Archives a Closed scorecard for long-term retention.</summary>
    public void Archive(CorrelationId correlationId, CausationId causationId)
    {
        if (Status != ScorecardStatus.Closed)
            throw new ScorecardInvalidStatusTransitionException(ScorecardId, Status.ToString(), ScorecardStatus.Archived.ToString(), correlationId, causationId);

        Status = ScorecardStatus.Archived;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IScorecardDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params ScorecardStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for Scorecard {ScorecardId}.");
    }
}
