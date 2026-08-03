// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Domain
//  File:           GlobalSafetyInsightsAggregate.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the GlobalSafetyInsights bounded context: an immutable,
//                  append-only, point-in-time cross-site safety-intelligence snapshot (KPIs, risk
//                  scores, anomalies, recommendations) computed over a SafetyAnalysisTimeWindow for
//                  a tenant's Region. Modeled as a Workflow-module-style maximalist aggregate
//                  (Handbook §2.5 Canonical Aggregate Rule) even though it has no further lifecycle
//                  transitions after Compute -- every computation run produces a brand-new snapshot
//                  row rather than mutating a prior one, so cross-site trend analysis (a later
//                  agent's dashboard) can diff snapshots over time. Additive-only evolution.
// ============================================================================
using System.Collections.ObjectModel;
using PlantSafety.Domain.GlobalSafetyInsights.Entities;
using PlantSafety.Domain.GlobalSafetyInsights.Events;
using PlantSafety.Domain.GlobalSafetyInsights.Rules;
using PlantSafety.Domain.GlobalSafetyInsights.ValueObjects;
using PlantSafety.Domain.GlobalSafetyInsights.ValueObjects.Ids;
using PlantSafety.SharedKernel.Contracts.Enterprise.MultiRegion;

namespace PlantSafety.Domain.GlobalSafetyInsights.Aggregates;

/// <summary>Aggregate root for a single computed cross-site safety-intelligence snapshot.</summary>
public sealed partial class GlobalSafetyInsightsAggregate
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this snapshot.</summary>
    public GlobalSafetyInsightsId GlobalSafetyInsightsId { get; private set; }

    /// <summary>Tenant that owns this snapshot. Every query/operation on this aggregate MUST be scoped by this value -- zero cross-tenant leakage (Phase 8 constraint).</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Deployment region this snapshot was computed for (Phase 7 Region smart enum, reused verbatim).</summary>
    public Region Region { get; private set; } = Region.Default;

    private readonly List<Guid> _siteIds = [];

    /// <summary>Sites covered by this snapshot (bare Guid references -- no first-class Site aggregate exists yet, per Phase 8 scope).</summary>
    public IReadOnlyList<Guid> SiteIds => _siteIds.AsReadOnly();

    /// <summary>Inclusive UTC analysis window this snapshot was computed over.</summary>
    public SafetyAnalysisTimeWindow TimeWindow { get; private set; }

    // ---- Computed intelligence ----------------------------------------------

    /// <summary>Cross-site KPI values (TRIR/LTIR/exposure rate/incident density/machine-state correlation).</summary>
    public GlobalSafetyKpiSnapshot Kpis { get; private set; } = null!;

    private readonly List<SiteRiskScore> _riskScores = [];

    /// <summary>Per-site/per-region/per-machine-family risk scores.</summary>
    public IReadOnlyList<SiteRiskScore> RiskScores => _riskScores.AsReadOnly();

    private readonly List<DetectedAnomaly> _anomalies = [];

    /// <summary>Cross-site/systemic anomalies detected within the analysis window.</summary>
    public IReadOnlyList<DetectedAnomaly> Anomalies => _anomalies.AsReadOnly();

    private readonly List<SafetyRecommendation> _recommendations = [];

    /// <summary>Structured, actionable recommendations derived from the anomalies/risk scores above.</summary>
    public IReadOnlyList<SafetyRecommendation> Recommendations => _recommendations.AsReadOnly();

    // ---- Audit trail ---------------------------------------------------------

    private readonly List<GlobalSafetyInsightsHistoryEntry> _historyEntries = [];

    /// <summary>Append-only audit trail: one entry per computation run plus one per received CrossSiteActionEvent.</summary>
    public IReadOnlyList<GlobalSafetyInsightsHistoryEntry> HistoryEntries => _historyEntries.AsReadOnly();

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC instant this snapshot was computed.</summary>
    public UtcTimestamp ComputedAtUtc { get; private set; }

    /// <summary>Identity of the user/system that triggered this computation.</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IGlobalSafetyInsightsDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IGlobalSafetyInsightsDomainEvent> DomainEvents
        => new ReadOnlyCollection<IGlobalSafetyInsightsDomainEvent>(_domainEvents);

    // For rehydration only.
    private GlobalSafetyInsightsAggregate() { }

    // ---- Factory: Compute ------------------------------------------------------

    /// <summary>
    /// Computes and validates a brand-new cross-site safety-intelligence snapshot. Raises
    /// <see cref="GlobalSafetyInsightComputedEvent"/>, one <see cref="GlobalSafetyRiskScoredEvent"/>
    /// per <paramref name="riskScores"/> entry, and one <see cref="GlobalSafetyAnomalyDetectedEvent"/>
    /// per <paramref name="anomalies"/> entry.
    /// </summary>
    /// <exception cref="Exceptions.GlobalSafetyInsightsInvalidComputationException">
    /// Thrown when <paramref name="tenantId"/>/<paramref name="siteIds"/>/<paramref name="kpis"/> violate a module invariant.
    /// </exception>
    public static GlobalSafetyInsightsAggregate Compute(
        Guid tenantId,
        Region region,
        IReadOnlyList<Guid> siteIds,
        SafetyAnalysisTimeWindow timeWindow,
        GlobalSafetyKpiSnapshot kpis,
        IReadOnlyList<SiteRiskScore> riskScores,
        IReadOnlyList<DetectedAnomaly> anomalies,
        IReadOnlyList<SafetyRecommendation> recommendations,
        UserId createdBy,
        UtcTimestamp nowUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
    {
        ArgumentNullException.ThrowIfNull(kpis);
        ArgumentNullException.ThrowIfNull(siteIds);

        GlobalSafetyInsightsInvariantRules.EnsureTenantIdPresent(tenantId, correlationId.Value, causationId.Value);
        GlobalSafetyInsightsInvariantRules.EnsureAtLeastOneSite(siteIds, correlationId.Value, causationId.Value);
        GlobalSafetyInsightsInvariantRules.EnsureKpisAreNonNegative(kpis, correlationId.Value, causationId.Value);

        var agg = new GlobalSafetyInsightsAggregate
        {
            GlobalSafetyInsightsId = GlobalSafetyInsightsId.New(),
            TenantId = tenantId,
            Region = region,
            TimeWindow = timeWindow,
            Kpis = kpis,
            ComputedAtUtc = nowUtc,
            CreatedBy = createdBy,
        };

        agg._siteIds.AddRange(siteIds.Distinct());
        agg._riskScores.AddRange(riskScores ?? []);
        agg._anomalies.AddRange(anomalies ?? []);
        agg._recommendations.AddRange(recommendations ?? []);

        agg._historyEntries.Add(new GlobalSafetyInsightsHistoryEntry(
            agg.GlobalSafetyInsightsId.Value, "InsightComputed",
            $"Snapshot computed for {agg._siteIds.Count} site(s) in region {region.Code} over {timeWindow}.",
            nowUtc, createdBy.Value));

        agg.AddDomainEvent(new GlobalSafetyInsightComputedEvent(
            GlobalSafetyInsightsEventId.New(), agg.GlobalSafetyInsightsId, tenantId, region.Code, timeWindow,
            agg._siteIds.Count, agg._anomalies.Count, agg._recommendations.Count, nowUtc, actor, correlationId, causationId));

        foreach (var score in agg._riskScores)
        {
            agg.AddDomainEvent(new GlobalSafetyRiskScoredEvent(
                GlobalSafetyInsightsEventId.New(), agg.GlobalSafetyInsightsId, tenantId, score, nowUtc, actor, correlationId, causationId));
        }

        foreach (var anomaly in agg._anomalies)
        {
            agg.AddDomainEvent(new GlobalSafetyAnomalyDetectedEvent(
                GlobalSafetyInsightsEventId.New(), agg.GlobalSafetyInsightsId, tenantId, anomaly, nowUtc, actor, correlationId, causationId));
        }

        return agg;
    }

    // ---- Cross-site action audit trail (Phase 8 §4 event mesh extension) -------

    /// <summary>
    /// Records a <see cref="CrossSiteActionEvent"/> into this snapshot's audit trail. Called by
    /// CrossSiteActionConsumer -- the Multi-Site Coordination workflow that raises
    /// CrossSiteActionEvent belongs to a later agent's build; this method is the real, working
    /// recording surface that agent's commands will ultimately drive.
    /// </summary>
    public void RecordCrossSiteAction(string actionType, string description, Guid? initiatedByUserId, UtcTimestamp nowUtc)
    {
        if (string.IsNullOrWhiteSpace(actionType))
            throw new ArgumentException("ActionType must not be empty.", nameof(actionType));

        _historyEntries.Add(new GlobalSafetyInsightsHistoryEntry(
            GlobalSafetyInsightsId.Value, "CrossSiteAction", $"{actionType}: {description}", nowUtc, initiatedByUserId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IGlobalSafetyInsightsDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
