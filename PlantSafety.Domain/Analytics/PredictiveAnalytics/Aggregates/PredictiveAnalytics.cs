// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.PredictiveAnalytics
//  Layer:          Domain
//  File:           PredictiveAnalytics.cs
//  Created:        2026-06-17
//  Updated:        2026-07-18 -- extended from identity-only scaffold with the full
//                  predictive-risk-model output record shape. NOTE: no dedicated
//                  field-level spec was found in project_plantsafety_domains.md
//                  (v4.0 20-module list); project_plantsafety_overview.md Section 5
//                  confirms "Predictive Analytics" (4.40) as a distinct Analytics
//                  sub-module, and ui_predictive_risk_dashboard_wireframe.md /
//                  ui_forecast_overview_wireframe.md establish predicted-risk-score
//                  and confidence-band concepts used across the platform's ML-facing
//                  UI. Properties below use industry-standard predictive-model output
//                  fields (model, prediction type, score, confidence, horizon) for
//                  this entity type.
//                  Additive-only evolution.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the PredictiveAnalytics bounded context. Tracks
//                  a single predictive-model output/scoring record: its model,
//                  prediction type, target entity, predicted risk score, confidence,
//                  and horizon.
//                  Additive-only evolution.
//  UPDATED:        2026-08-02 -- Phase 4 full build-out: added Review/Action/Close/Archive
//                  lifecycle methods, grounded in the status vocabulary documented on
//                  PredictiveAnalyticsItemDto.Status ("Generated/Reviewed/Actioned"). Previously
//                  this aggregate had only a Create factory -- PredictiveAnalyticsStatus/
//                  PredictiveAnalyticsLifecycleRules were fully scaffolded but nothing called
//                  them. Mirrors Permits.cs's lifecycle shape and guard/event/exception
//                  conventions.
// ============================================================================
namespace PlantSafety.Domain.Analytics.PredictiveAnalytics.Aggregates;

/// <summary>Aggregate root for the PredictiveAnalytics bounded context.</summary>
public sealed partial class PredictiveAnalytics
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this PredictiveAnalytics aggregate.</summary>
    public PredictiveAnalyticsId PredictiveAnalyticsId { get; private set; }

    /// <summary>Tenant that owns this prediction record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this prediction is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public PredictiveAnalyticsStatus Status { get; private set; }

    // ---- Predictive-analytics-specific business properties ----------------------------

    /// <summary>Name of the predictive model that generated this output.</summary>
    public string ModelName { get; private set; } = string.Empty;

    /// <summary>Prediction classification (e.g., IncidentRisk, EquipmentFailure, ComplianceDrift).</summary>
    public string PredictionType { get; private set; } = string.Empty;

    /// <summary>Entity this prediction targets (worker, equipment, site), when applicable.</summary>
    public Guid? TargetEntityId { get; private set; }

    /// <summary>Predicted risk score (0-100).</summary>
    public decimal PredictedRiskScore { get; private set; }

    /// <summary>Model confidence level for this prediction (0-100).</summary>
    public decimal ConfidenceLevel { get; private set; }

    /// <summary>Prediction horizon, in days, this output forecasts over.</summary>
    public int PredictionHorizonDays { get; private set; } = 30;

    /// <summary>UTC timestamp this prediction was generated.</summary>
    public UtcTimestamp GeneratedAtUtc { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IPredictiveAnalyticsDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IPredictiveAnalyticsDomainEvent> DomainEvents
        => new ReadOnlyCollection<IPredictiveAnalyticsDomainEvent>(_domainEvents);

    // For rehydration only.
    private PredictiveAnalytics() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new PredictiveAnalytics aggregate and raises <see cref="PredictiveAnalyticsCreatedEvent"/>.</summary>
    public static PredictiveAnalytics Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string modelName = "",
        string predictionType = "",
        Guid? targetEntityId = null,
        decimal predictedRiskScore = 0m,
        decimal confidenceLevel = 0m,
        int predictionHorizonDays = 30)
    {
        var agg = new PredictiveAnalytics
        {
            PredictiveAnalyticsId    = PredictiveAnalyticsId.New(),
            TenantId                 = tenantId,
            SiteId                   = siteId,
            Status                   = PredictiveAnalyticsStatus.Created,
            ModelName                = (modelName ?? string.Empty).Trim(),
            PredictionType           = (predictionType ?? string.Empty).Trim(),
            TargetEntityId           = targetEntityId,
            PredictedRiskScore       = predictedRiskScore,
            ConfidenceLevel          = confidenceLevel,
            PredictionHorizonDays    = predictionHorizonDays < 1 ? 1 : predictionHorizonDays,
            GeneratedAtUtc           = createdAtUtc,
            CreatedAtUtc             = createdAtUtc,
            CreatedBy                = createdBy,
        };

        agg.AddDomainEvent(new PredictiveAnalyticsCreatedEvent(
            EventId:       new PredictiveAnalyticsEventId(Uuid7.New()),
            PredictiveAnalyticsId:      agg.PredictiveAnalyticsId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Lifecycle: Review / Action -------------------------------------------

    /// <summary>
    /// Reviews a generated prediction (Created -&gt; Active, i.e. "Reviewed"). Valid only from
    /// <see cref="PredictiveAnalyticsStatus.Created"/>.
    /// </summary>
    public void Review(
        UserId reviewedBy,
        UtcTimestamp reviewedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, PredictiveAnalyticsLifecycleRules.CanActivate, PredictiveAnalyticsStatus.Created);

        Status = PredictiveAnalyticsStatus.Active;

        AddDomainEvent(new PredictiveAnalyticsReviewedEvent(
            EventId: new PredictiveAnalyticsEventId(Uuid7.New()),
            PredictiveAnalyticsId: PredictiveAnalyticsId,
            ReviewedByUserId: reviewedBy,
            ReviewedAtUtc: reviewedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Records that a reviewed prediction has been acted upon (Active -&gt; Completed, i.e.
    /// "Actioned"). Valid only from <see cref="PredictiveAnalyticsStatus.Active"/>.
    /// </summary>
    public void Action(
        UserId actionedBy,
        string? actionNotes,
        UtcTimestamp actionedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, PredictiveAnalyticsLifecycleRules.CanComplete, PredictiveAnalyticsStatus.Active);

        Status = PredictiveAnalyticsStatus.Completed;

        AddDomainEvent(new PredictiveAnalyticsActionedEvent(
            EventId: new PredictiveAnalyticsEventId(Uuid7.New()),
            PredictiveAnalyticsId: PredictiveAnalyticsId,
            ActionedByUserId: actionedBy,
            ActionNotes: string.IsNullOrWhiteSpace(actionNotes) ? null : actionNotes.Trim(),
            ActionedAtUtc: actionedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Close / Archive -------------------------------------------

    /// <summary>
    /// Formally closes an actioned prediction (Completed -&gt; Closed). Valid only from
    /// <see cref="PredictiveAnalyticsStatus.Completed"/>.
    /// </summary>
    public void Close(
        UserId closedBy,
        UtcTimestamp closedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, PredictiveAnalyticsLifecycleRules.CanClose, PredictiveAnalyticsStatus.Completed);

        Status = PredictiveAnalyticsStatus.Closed;

        AddDomainEvent(new PredictiveAnalyticsClosedEvent(
            EventId: new PredictiveAnalyticsEventId(Uuid7.New()),
            PredictiveAnalyticsId: PredictiveAnalyticsId,
            ClosedByUserId: closedBy,
            ClosedAtUtc: closedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Archives a closed prediction for long-term retention (Closed -&gt; Archived). Valid only
    /// from <see cref="PredictiveAnalyticsStatus.Closed"/>. See
    /// <see cref="Policies.PredictiveAnalyticsArchivalPolicy"/> for eligibility outside the
    /// aggregate boundary (e.g. batch archival jobs).
    /// </summary>
    public void Archive(
        UtcTimestamp archivedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, PredictiveAnalyticsLifecycleRules.CanArchive, PredictiveAnalyticsStatus.Closed);

        Status = PredictiveAnalyticsStatus.Archived;

        AddDomainEvent(new PredictiveAnalyticsArchivedEvent(
            EventId: new PredictiveAnalyticsEventId(Uuid7.New()),
            PredictiveAnalyticsId: PredictiveAnalyticsId,
            ArchivedAtUtc: archivedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IPredictiveAnalyticsDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params PredictiveAnalyticsStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for PredictiveAnalytics {PredictiveAnalyticsId}.");
    }

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, Func<PredictiveAnalyticsStatus, bool> guard, PredictiveAnalyticsStatus expected)
    {
        if (guard(Status)) return;

        throw new PredictiveAnalyticsInvalidStatusTransitionException(
            PredictiveAnalyticsId, Status.ToString(), expected.ToString(), correlationId, causationId);
    }
}
