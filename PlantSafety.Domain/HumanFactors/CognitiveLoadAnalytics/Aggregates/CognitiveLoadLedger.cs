// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Domain
//  File:           CognitiveLoadLedger.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for a single tenant/site/shift's cognitive-workload measurement
//                  ledger: worker cognitive-load readings, task-complexity profiles, and
//                  shift-level workload-trend recalculations. Sits alongside (not inside) the
//                  pre-existing generic HumanFactors aggregate -- this aggregate is the
//                  auditable substrate CognitiveRiskPrediction is generated from, mirroring
//                  Sustainability.AdvancedMetrics.AdvancedMetricsLedger's precedent for a
//                  ledger-style sub-aggregate sitting beside a thin generic parent aggregate.
//                  Data governance: individual worker cognitive-load data is sensitive personal
//                  data -- access is restricted at the query/authorization layer, cross-worker
//                  dashboards use aggregated read-models only, and retention is limited to active
//                  employment plus the HR-policy-defined window (Archive lifecycle step).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Aggregates;

/// <summary>Aggregate root for a tenant/site/shift's cognitive-workload measurement ledger.</summary>
public sealed class CognitiveLoadLedger
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this ledger aggregate.</summary>
    public CognitiveLoadLedgerId CognitiveLoadLedgerId { get; private set; }

    /// <summary>Tenant that owns this ledger.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this ledger is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Shift classification this ledger is scoped to (Scheduling shift-pattern correlation hook).</summary>
    public ShiftType ShiftType { get; private set; }

    /// <summary>UTC start-of-shift instant this ledger covers.</summary>
    public UtcTimestamp ShiftDateUtc { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public CognitiveLoadLedgerStatus Status { get; private set; }

    // ---- Child records ---------------------------------------------------------

    private readonly List<CognitiveLoadReading> _cognitiveLoadReadings = [];
    private readonly List<TaskComplexityProfile> _taskComplexityProfiles = [];
    private readonly List<WorkloadTrendRecord> _workloadTrendRecords = [];

    /// <summary>Cognitive-load readings captured in this shift's ledger.</summary>
    public IReadOnlyCollection<CognitiveLoadReading> CognitiveLoadReadings => _cognitiveLoadReadings.AsReadOnly();

    /// <summary>Task-complexity profiles defined against this shift's ledger.</summary>
    public IReadOnlyCollection<TaskComplexityProfile> TaskComplexityProfiles => _taskComplexityProfiles.AsReadOnly();

    /// <summary>Workload-trend recalculation snapshots for this shift's ledger.</summary>
    public IReadOnlyCollection<WorkloadTrendRecord> WorkloadTrendRecords => _workloadTrendRecords.AsReadOnly();

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<ICognitiveLoadAnalyticsDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<ICognitiveLoadAnalyticsDomainEvent> DomainEvents
        => new ReadOnlyCollection<ICognitiveLoadAnalyticsDomainEvent>(_domainEvents);

    // For rehydration only.
    private CognitiveLoadLedger() { }

    // ---- Factory: Create (Open) -----------------------------------------------------------

    /// <summary>Opens a new CognitiveLoadLedger for the given tenant/site/shift.</summary>
    public static CognitiveLoadLedger Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        ShiftType shiftType,
        UtcTimestamp shiftDateUtc)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("TenantId must not be empty.", nameof(tenantId));

        return new CognitiveLoadLedger
        {
            CognitiveLoadLedgerId = CognitiveLoadLedgerId.New(),
            TenantId               = tenantId,
            SiteId                 = siteId,
            ShiftType               = shiftType,
            ShiftDateUtc             = shiftDateUtc,
            Status                   = CognitiveLoadLedgerStatus.Open,
            CreatedAtUtc             = createdAtUtc,
            CreatedBy                = createdBy,
        };
        // Deliberately no "Ledger opened" event, mirroring AdvancedMetricsLedger.Create: the
        // ledger is an implicit measurement substrate; the first recorded reading is the first
        // observable domain event.
    }

    // ---- Behavior: RecordCognitiveLoadReading -----------------------------------------------

    /// <summary>
    /// Records a single worker cognitive-workload measurement, classifying it via
    /// <see cref="CognitiveOverloadPolicy"/> and raising <see cref="CognitiveOverloadDetectedEvent"/>
    /// when the classification is <see cref="CognitiveWorkloadLevel.Overload"/>.
    /// </summary>
    public CognitiveLoadReading RecordCognitiveLoadReading(
        UserId workerUserId,
        TaskComplexityProfileId? taskComplexityProfileId,
        decimal cognitiveLoadScore,
        string measurementMethod,
        UtcTimestamp measuredAtUtc,
        UtcTimestamp recordedAtUtc,
        UserId recordedBy,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureCanRecord(correlationId, causationId);

        var level = CognitiveOverloadPolicy.Classify(cognitiveLoadScore);

        var reading = new CognitiveLoadReading(
            CognitiveLoadReadingId.New(), workerUserId, taskComplexityProfileId, cognitiveLoadScore, level,
            measurementMethod, measuredAtUtc, recordedAtUtc, recordedBy);

        _cognitiveLoadReadings.Add(reading);

        AddDomainEvent(new CognitiveLoadReadingRecordedEvent(
            CognitiveLoadAnalyticsEventId.New(), CognitiveLoadLedgerId, reading.CognitiveLoadReadingId,
            workerUserId.Value, cognitiveLoadScore, level, recordedAtUtc, actor, correlationId, causationId));

        if (level == CognitiveWorkloadLevel.Overload)
        {
            AddDomainEvent(new CognitiveOverloadDetectedEvent(
                CognitiveLoadAnalyticsEventId.New(), CognitiveLoadLedgerId, reading.CognitiveLoadReadingId,
                workerUserId.Value, cognitiveLoadScore, recordedAtUtc, actor, correlationId, causationId));
        }

        return reading;
    }

    // ---- Behavior: DefineTaskComplexityProfile -----------------------------------------------

    /// <summary>Defines a task-complexity profile against this ledger's shift.</summary>
    public TaskComplexityProfile DefineTaskComplexityProfile(
        string taskCode,
        string taskName,
        int complexityScore,
        string description,
        UtcTimestamp definedAtUtc,
        UserId definedBy,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureCanRecord(correlationId, causationId);

        var profile = new TaskComplexityProfile(
            TaskComplexityProfileId.New(), taskCode, taskName, complexityScore, description, definedAtUtc, definedBy);

        _taskComplexityProfiles.Add(profile);

        // Deliberately no domain event: definitional reference data, mirrors this aggregate's
        // Create() "silent substrate" precedent -- correlated readings are the observable signal.

        return profile;
    }

    // ---- Behavior: RecalculateWorkloadTrend -----------------------------------------------

    /// <summary>
    /// Recalculates the shift's workload trend from all readings whose MeasuredAtUtc falls within
    /// [<paramref name="periodStartUtc"/>, <paramref name="periodEndUtc"/>]. Backs the periodic
    /// per-shift-cycle recalculation scheduling hook.
    /// </summary>
    public WorkloadTrendRecord RecalculateWorkloadTrend(
        UtcTimestamp periodStartUtc,
        UtcTimestamp periodEndUtc,
        UtcTimestamp recalculatedAtUtc,
        UserId recalculatedBy,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureCanRecord(correlationId, causationId);

        var windowed = _cognitiveLoadReadings
            .Where(r => r.MeasuredAtUtc.Value >= periodStartUtc.Value && r.MeasuredAtUtc.Value <= periodEndUtc.Value)
            .ToList();

        var meanScore = windowed.Count == 0 ? 0m : Math.Round(windowed.Average(r => r.CognitiveLoadScore), 2, MidpointRounding.AwayFromZero);
        var overloadCount = windowed.Count(r => r.IsOverload);

        var record = new WorkloadTrendRecord(
            WorkloadTrendRecordId.New(), periodStartUtc, periodEndUtc, meanScore, overloadCount, windowed.Count,
            recalculatedAtUtc, recalculatedBy);

        _workloadTrendRecords.Add(record);

        AddDomainEvent(new WorkloadTrendRecalculatedEvent(
            CognitiveLoadAnalyticsEventId.New(), CognitiveLoadLedgerId, record.WorkloadTrendRecordId,
            meanScore, overloadCount, windowed.Count, recalculatedAtUtc, actor, correlationId, causationId));

        return record;
    }

    // ---- Behavior: Close -----------------------------------------------------------

    /// <summary>Closes the ledger at the end of its shift, stopping further record intake.</summary>
    public void Close(UtcTimestamp atUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (!CognitiveLoadLedgerLifecycleRules.CanClose(Status))
            throw new CognitiveLoadLedgerInvalidStatusTransitionException(
                CognitiveLoadLedgerId, Status.ToString(), CognitiveLoadLedgerStatus.Closed.ToString(), correlationId, causationId);

        Status = CognitiveLoadLedgerStatus.Closed;
    }

    // ---- Behavior: Archive -----------------------------------------------------------

    /// <summary>Archives a closed ledger per the HR data-retention policy (active employment + defined window).</summary>
    public void Archive(UtcTimestamp atUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (!CognitiveLoadLedgerLifecycleRules.CanArchive(Status))
            throw new CognitiveLoadLedgerInvalidStatusTransitionException(
                CognitiveLoadLedgerId, Status.ToString(), CognitiveLoadLedgerStatus.Archived.ToString(), correlationId, causationId);

        Status = CognitiveLoadLedgerStatus.Archived;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(ICognitiveLoadAnalyticsDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureCanRecord(CorrelationId correlationId, CausationId causationId)
    {
        if (!CognitiveLoadLedgerLifecycleRules.CanRecord(Status))
            throw new CognitiveLoadLedgerInvalidStatusTransitionException(
                CognitiveLoadLedgerId, Status.ToString(), CognitiveLoadLedgerStatus.Open.ToString(), correlationId, causationId);
    }
}
