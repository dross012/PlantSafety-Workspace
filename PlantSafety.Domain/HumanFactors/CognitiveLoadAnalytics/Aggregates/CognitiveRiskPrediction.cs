// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Domain
//  File:           CognitiveRiskPrediction.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for a single generated predictive cognitive-risk assessment for
//                  a worker, derived from a source CognitiveLoadLedger's recorded readings/trend.
//                  Elevated/Critical predictions are the risk hook consumed by RiskManagement and
//                  correlated against AccidentManagement root-cause data; the Acknowledge lifecycle
//                  step backs the acknowledgement-response-time KPI. Mirrors
//                  Sustainability.AdvancedMetrics.EsgDisclosureReport's precedent for a
//                  separately-lifecycled aggregate generated from a source ledger.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Aggregates;

/// <summary>Aggregate root for a single generated, worker-scoped predictive cognitive-risk assessment.</summary>
public sealed class CognitiveRiskPrediction
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this prediction aggregate.</summary>
    public CognitiveRiskPredictionId CognitiveRiskPredictionId { get; private set; }

    /// <summary>Tenant that owns this prediction.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this prediction is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Identity of the worker this prediction was generated for.</summary>
    public UserId WorkerUserId { get; private set; }

    /// <summary>Identity of the source CognitiveLoadLedger this prediction was derived from.</summary>
    public CognitiveLoadLedgerId SourceLedgerId { get; private set; }

    /// <summary>Predicted cognitive-risk classification.</summary>
    public CognitiveRiskLevel PredictedRiskLevel { get; private set; }

    /// <summary>Normalized (0-1) prediction confidence score.</summary>
    public decimal PredictionScore { get; private set; }

    /// <summary>Version identifier of the predictive model used (feeds the retraining schedule/versioning trail).</summary>
    public string ModelVersion { get; private set; } = string.Empty;

    /// <summary>Narrative basis for the prediction (which inputs/thresholds drove the classification).</summary>
    public string PredictionBasis { get; private set; } = string.Empty;

    /// <summary>Current lifecycle status.</summary>
    public CognitiveRiskPredictionStatus Status { get; private set; }

    /// <summary>UTC instant the prediction was generated.</summary>
    public UtcTimestamp GeneratedAtUtc { get; private set; }

    /// <summary>Identity of the user (or scheduled job actor) who generated this prediction.</summary>
    public UserId GeneratedBy { get; private set; }

    /// <summary>UTC instant the alert was acknowledged, if it has been.</summary>
    public UtcTimestamp? AcknowledgedAtUtc { get; private set; }

    /// <summary>Identity of the user who acknowledged the alert, if any.</summary>
    public UserId? AcknowledgedBy { get; private set; }

    /// <summary>Free-text acknowledgement notes, if any.</summary>
    public string? AcknowledgementNotes { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<ICognitiveLoadAnalyticsDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<ICognitiveLoadAnalyticsDomainEvent> DomainEvents
        => new ReadOnlyCollection<ICognitiveLoadAnalyticsDomainEvent>(_domainEvents);

    // For rehydration only.
    private CognitiveRiskPrediction() { }

    // ---- Factory: Generate -----------------------------------------------------------

    /// <summary>Generates a new cognitive-risk prediction for a worker from a source ledger's computed inputs.</summary>
    public static CognitiveRiskPrediction Generate(
        UserId generatedBy,
        UtcTimestamp generatedAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        UserId workerUserId,
        CognitiveLoadLedgerId sourceLedgerId,
        CognitiveRiskLevel predictedRiskLevel,
        decimal predictionScore,
        string modelVersion,
        string predictionBasis)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("TenantId must not be empty.", nameof(tenantId));
        if (predictionScore is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(predictionScore), "PredictionScore must be between 0 and 1.");
        if (string.IsNullOrWhiteSpace(modelVersion))
            throw new ArgumentException("ModelVersion must not be null or whitespace.", nameof(modelVersion));

        var prediction = new CognitiveRiskPrediction
        {
            CognitiveRiskPredictionId = CognitiveRiskPredictionId.New(),
            TenantId                   = tenantId,
            SiteId                     = siteId,
            WorkerUserId                = workerUserId,
            SourceLedgerId               = sourceLedgerId,
            PredictedRiskLevel           = predictedRiskLevel,
            PredictionScore               = predictionScore,
            ModelVersion                   = modelVersion.Trim(),
            PredictionBasis                = (predictionBasis ?? string.Empty).Trim(),
            Status                          = CognitiveRiskPredictionStatus.Generated,
            GeneratedAtUtc                   = generatedAtUtc,
            GeneratedBy                       = generatedBy,
        };

        prediction.AddDomainEvent(new CognitiveRiskPredictionGeneratedEvent(
            CognitiveLoadAnalyticsEventId.New(), prediction.CognitiveRiskPredictionId, workerUserId.Value,
            sourceLedgerId, predictedRiskLevel, predictionScore, modelVersion.Trim(),
            generatedAtUtc, actor, correlationId, causationId));

        return prediction;
    }

    // ---- Behavior: Acknowledge -----------------------------------------------------------

    /// <summary>Acknowledges the prediction/alert. Backs the acknowledgement-response-time KPI.</summary>
    public void Acknowledge(
        UserId acknowledgedBy,
        string? notes,
        UtcTimestamp atUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        if (!CognitiveRiskPredictionLifecycleRules.CanAcknowledge(Status))
            throw new CognitiveRiskPredictionInvalidStatusTransitionException(
                CognitiveRiskPredictionId, Status.ToString(), CognitiveRiskPredictionStatus.Acknowledged.ToString(), correlationId, causationId);

        Status                 = CognitiveRiskPredictionStatus.Acknowledged;
        AcknowledgedAtUtc       = atUtc;
        AcknowledgedBy           = acknowledgedBy;
        AcknowledgementNotes      = (notes ?? string.Empty).Trim() is { Length: > 0 } trimmed ? trimmed : null;

        AddDomainEvent(new CognitiveRiskAlertAcknowledgedEvent(
            CognitiveLoadAnalyticsEventId.New(), CognitiveRiskPredictionId, acknowledgedBy.Value,
            atUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Dismiss -----------------------------------------------------------

    /// <summary>Dismisses the prediction as not actionable (reviewed, no alert response required).</summary>
    public void Dismiss(UtcTimestamp atUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (!CognitiveRiskPredictionLifecycleRules.CanDismiss(Status))
            throw new CognitiveRiskPredictionInvalidStatusTransitionException(
                CognitiveRiskPredictionId, Status.ToString(), CognitiveRiskPredictionStatus.Dismissed.ToString(), correlationId, causationId);

        Status = CognitiveRiskPredictionStatus.Dismissed;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(ICognitiveLoadAnalyticsDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
