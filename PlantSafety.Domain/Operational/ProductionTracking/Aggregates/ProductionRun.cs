// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.ProductionTracking
//  Layer:          Domain
//  File:           ProductionRun.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the ProductionRun bounded context -- backs the
//                  ProductionTracking UI's Dashboard/Run Detail/Batch Tracking/Timeline/KPI
//                  Dashboard/Event History pages (v1/production-tracking/*), which previously had
//                  no Application/API/Domain layer at all (see docs/Phase5/ui-gaps-scoping.md
//                  "ProductionTracking" section -- the largest, riskiest of the 9 gaps). Runs
//                  against a ProductionLine (master data) and a telemetry-monitored MachineId.
//                  Accumulates running totals (good/scrap units, downtime minutes) that the
//                  Application layer feeds into the real Phase 5
//                  Analytics.Dashboard.Kpi.ProductionKpiEngine (OEE/MTBF/MTTR/Throughput/Yield/
//                  ScrapRate) and Observability.Telemetry.TelemetryAnomalyDetector (bottleneck
//                  detection) rather than reimplementing KPI/anomaly math here. Mirrors
//                  PlantSafety.Domain.Operational.WorkOrders.Aggregates.WorkOrder's
//                  guarded-transition/domain-event/exception conventions. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Operational.ProductionTracking.Enums;
using PlantSafety.Domain.Operational.ProductionTracking.Events;
using PlantSafety.Domain.Operational.ProductionTracking.Exceptions;
using PlantSafety.Domain.Operational.ProductionTracking.Rules;
using PlantSafety.Domain.Operational.ProductionTracking.ValueObjects.Ids;

namespace PlantSafety.Domain.Operational.ProductionTracking.Aggregates;

/// <summary>Aggregate root for the ProductionRun bounded context.</summary>
public sealed class ProductionRun
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this ProductionRun aggregate.</summary>
    public ProductionRunId ProductionRunId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public ProductionRunStatus Status { get; private set; }

    // ---- Run-specific business properties -------------------------------------

    /// <summary>Human-readable run code (format PR-###).</summary>
    public string RunCode { get; private set; } = string.Empty;

    /// <summary>Identifier of the ProductionLine this run executes on.</summary>
    public ProductionLineId LineId { get; private set; }

    /// <summary>Display name of the line, captured at start for read-model convenience.</summary>
    public string LineName { get; private set; } = string.Empty;

    /// <summary>Identifier of the telemetry-monitored machine driving this run.</summary>
    public Guid MachineId { get; private set; }

    /// <summary>Operator display name.</summary>
    public string OperatorName { get; private set; } = string.Empty;

    /// <summary>
    /// Rated/nameplate ideal cycle time (seconds) for this run, feeding the OEE Performance factor
    /// (see PlantSafety.Domain.Analytics.Dashboard.Kpi.Services.OeeCalculator) and the
    /// TelemetryAnomalyDetector's cycle-time-degradation check.
    /// </summary>
    public double IdealCycleTimeSeconds { get; private set; }

    // ---- Running totals (fed into ProductionKpiEngine at query time) ----------

    /// <summary>Cumulative count of units produced that passed quality inspection.</summary>
    public int GoodUnitCount { get; private set; }

    /// <summary>Cumulative count of units scrapped/rejected.</summary>
    public int ScrapUnitCount { get; private set; }

    /// <summary>Cumulative downtime minutes recorded against this run.</summary>
    public double DowntimeMinutesTotal { get; private set; }

    // ---- Lifecycle timestamps ---------------------------------------------------

    /// <summary>UTC instant the run started.</summary>
    public UtcTimestamp StartedAtUtc { get; private set; }

    /// <summary>UTC instant the run ended (Completed/Failed), or null while still active.</summary>
    public UtcTimestamp? EndedAtUtc { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user/system that created this record.</summary>
    public UserId CreatedBy { get; private set; }

    /// <summary>UTC instant of the most recent update to this record.</summary>
    public UtcTimestamp UpdatedAtUtc { get; private set; }

    // ---- Domain Events ---------------------------------------------------------

    private readonly List<IProductionTrackingDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IProductionTrackingDomainEvent> DomainEvents
        => new ReadOnlyCollection<IProductionTrackingDomainEvent>(_domainEvents);

    // For rehydration only.
    private ProductionRun() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Starts a new ProductionRun and records <see cref="ProductionRunStartedEvent"/>.</summary>
    public static ProductionRun Start(
        UserId createdBy,
        UtcTimestamp startedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId,
        Guid tenantId,
        Guid? siteId,
        string runCode,
        ProductionLineId lineId,
        string lineName,
        Guid machineId,
        string operatorName,
        double idealCycleTimeSeconds)
    {
        if (string.IsNullOrWhiteSpace(runCode))
            throw new ArgumentException("RunCode must be supplied.", nameof(runCode));
        if (machineId == Guid.Empty)
            throw new ArgumentException("MachineId must be supplied.", nameof(machineId));
        if (idealCycleTimeSeconds <= 0)
            throw new ArgumentOutOfRangeException(nameof(idealCycleTimeSeconds), "IdealCycleTimeSeconds must be positive.");

        var agg = new ProductionRun
        {
            ProductionRunId = ProductionRunId.New(),
            TenantId = tenantId,
            SiteId = siteId,
            Status = ProductionRunStatus.Running,
            RunCode = runCode.Trim(),
            LineId = lineId,
            LineName = (lineName ?? string.Empty).Trim(),
            MachineId = machineId,
            OperatorName = (operatorName ?? string.Empty).Trim(),
            IdealCycleTimeSeconds = idealCycleTimeSeconds,
            GoodUnitCount = 0,
            ScrapUnitCount = 0,
            DowntimeMinutesTotal = 0,
            StartedAtUtc = startedAtUtc,
            CreatedAtUtc = startedAtUtc,
            CreatedBy = createdBy,
            UpdatedAtUtc = startedAtUtc,
        };

        agg.AddDomainEvent(new ProductionRunStartedEvent(
            EventId: Guid.NewGuid(),
            ProductionRunId: agg.ProductionRunId,
            LineId: lineId,
            MachineId: machineId,
            RunCode: agg.RunCode,
            StartedAtUtc: startedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));

        return agg;
    }

    // ---- Pause / Resume ---------------------------------------------------------

    /// <summary>Pauses an actively-running run.</summary>
    public void Pause(string? reason, UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
        => TransitionStatus(ProductionRunStatus.Paused, ProductionRunLifecycleRules.CanPause, reason, occurredAtUtc, actor, correlationId, causationId);

    /// <summary>Resumes a paused run.</summary>
    public void Resume(UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
        => TransitionStatus(ProductionRunStatus.Running, ProductionRunLifecycleRules.CanResume, null, occurredAtUtc, actor, correlationId, causationId);

    private void TransitionStatus(
        ProductionRunStatus target,
        Func<ProductionRunStatus, bool> guard,
        string? reason,
        UtcTimestamp occurredAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        if (!guard(Status))
        {
            throw new ProductionRunInvalidStatusTransitionException(
                ProductionRunId, Status.ToString(), target.ToString(), correlationId, causationId);
        }

        var previous = Status;
        Status = target;
        UpdatedAtUtc = occurredAtUtc;

        AddDomainEvent(new ProductionRunStatusChangedEvent(
            EventId: Guid.NewGuid(),
            ProductionRunId: ProductionRunId,
            PreviousStatus: previous,
            NewStatus: target,
            Reason: reason,
            ChangedAtUtc: occurredAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Timeline events (also feeds bottleneck detection when IsDowntime) ------

    /// <summary>
    /// Records a timeline event against this run (Step/Pause/Error/Resume/Downtime/etc.), optionally
    /// accumulating good/scrap unit counts and downtime minutes into the run's running totals.
    /// </summary>
    public void RecordEvent(
        string runEventType,
        string description,
        bool isDowntime,
        string? downtimeReasonCode,
        double downtimeMinutes,
        int goodUnitsDelta,
        int scrapUnitsDelta,
        UtcTimestamp occurredAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        if (!ProductionRunLifecycleRules.CanRecordEvent(Status))
        {
            throw new ProductionRunInvalidStatusTransitionException(
                ProductionRunId, Status.ToString(), "EventRecorded", correlationId, causationId);
        }
        if (string.IsNullOrWhiteSpace(runEventType))
            throw new ArgumentException("RunEventType must be supplied.", nameof(runEventType));
        if (goodUnitsDelta < 0)
            throw new ArgumentOutOfRangeException(nameof(goodUnitsDelta));
        if (scrapUnitsDelta < 0)
            throw new ArgumentOutOfRangeException(nameof(scrapUnitsDelta));
        if (downtimeMinutes < 0)
            throw new ArgumentOutOfRangeException(nameof(downtimeMinutes));

        GoodUnitCount += goodUnitsDelta;
        ScrapUnitCount += scrapUnitsDelta;
        if (isDowntime)
            DowntimeMinutesTotal += downtimeMinutes;
        UpdatedAtUtc = occurredAtUtc;

        AddDomainEvent(new ProductionRunEventRecordedEvent(
            EventId: Guid.NewGuid(),
            ProductionRunId: ProductionRunId,
            RunEventType: runEventType.Trim(),
            Description: (description ?? string.Empty).Trim(),
            IsDowntime: isDowntime,
            DowntimeReasonCode: string.IsNullOrWhiteSpace(downtimeReasonCode) ? null : downtimeReasonCode.Trim(),
            DowntimeMinutes: downtimeMinutes,
            OccurredAtUtc: occurredAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Batches -----------------------------------------------------------

    /// <summary>Adds a new Batch to this run.</summary>
    public BatchId AddBatch(
        string batchCode,
        BatchStatus status,
        UtcTimestamp startedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        if (!ProductionRunLifecycleRules.CanAddBatch(Status))
        {
            throw new ProductionRunInvalidStatusTransitionException(
                ProductionRunId, Status.ToString(), "BatchAdded", correlationId, causationId);
        }
        if (string.IsNullOrWhiteSpace(batchCode))
            throw new ArgumentException("BatchCode must be supplied.", nameof(batchCode));

        var batchId = BatchId.New();
        UpdatedAtUtc = startedAtUtc;

        AddDomainEvent(new ProductionBatchAddedEvent(
            EventId: Guid.NewGuid(),
            ProductionRunId: ProductionRunId,
            BatchId: batchId,
            BatchCode: batchCode.Trim(),
            Status: status,
            StartedAtUtc: startedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));

        return batchId;
    }

    // ---- Completion --------------------------------------------------------

    /// <summary>Ends the run, either successfully (Completed) or abnormally (Failed).</summary>
    public void End(
        bool succeeded,
        UtcTimestamp endedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        if (!ProductionRunLifecycleRules.CanEnd(Status))
        {
            throw new ProductionRunInvalidStatusTransitionException(
                ProductionRunId, Status.ToString(), succeeded ? "Completed" : "Failed", correlationId, causationId);
        }

        Status = succeeded ? ProductionRunStatus.Completed : ProductionRunStatus.Failed;
        EndedAtUtc = endedAtUtc;
        UpdatedAtUtc = endedAtUtc;

        AddDomainEvent(new ProductionRunCompletedEvent(
            EventId: Guid.NewGuid(),
            ProductionRunId: ProductionRunId,
            FinalStatus: Status,
            GoodUnitCount: GoodUnitCount,
            ScrapUnitCount: ScrapUnitCount,
            EndedAtUtc: endedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IProductionTrackingDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
