/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               SafetyPrediction.cs
    DESCRIPTION:        SAFETY-CRITICAL Aggregate Root for the PredictiveSafety module. Represents a single
                        AI-generated safety prediction for a plant area, process, or equipment item.

                        ENFORCED SAFETY INVARIANTS (these are non-negotiable; do NOT remove or relax):
                          4.4 — Confidence floor: predictions with score < MinimumConfidenceScore (0.70) cannot
                                drive automated safety actions. The floor may only be raised, never lowered.
                          4.5 — Fail safe, not fail open: uncertain predictions are blocked, never passed through.
                                A prediction that cannot be validated is treated as a non-prediction.
                          4.6 — Explainability: all predictions must carry at least one evidence item with a
                                human-readable description before they can be published.
                          4.7 — Human decisions are never overridden: once acknowledged or dismissed by a human,
                                the prediction is terminal. No automated process may undo a human decision.
                          Dual validation: High and Critical risk predictions require a second validation pass
                                before a safety alert is raised. (SafetyPredictionDomainService enforces this.)

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only. Safety invariants may only be tightened, never loosened.
==========================================================================================================================*/

using Intelligence.Domain.Shared.Abstractions;
using Intelligence.Domain.Shared.Constants;
using Intelligence.Domain.Shared.Exceptions;
using Intelligence.Domain.PredictiveSafety.Entities;
using Intelligence.Domain.PredictiveSafety.Events;
using Intelligence.Domain.PredictiveSafety.ValueObjects;

namespace Intelligence.Domain.PredictiveSafety.Aggregates;

/// <summary>
/// SAFETY-CRITICAL aggregate root. Represents an AI-generated safety prediction.
/// All safety invariants from Rules 4.4–4.7 are enforced at this boundary.
/// </summary>
public sealed class SafetyPrediction : AggregateRoot<SafetyPredictionId>
{
    private readonly List<PredictionEvidence> _evidence = new();

    /// <summary>The subject of this prediction (plant area, process, or equipment).</summary>
    public PredictionSubject Subject { get; private init; }

    /// <summary>The assessed risk level for this prediction.</summary>
    public RiskLevel RiskLevel { get; private set; }

    /// <summary>SAFETY-CRITICAL: The confidence score. Below 0.70 blocks automated actions (Rule 4.4).</summary>
    public ConfidenceScore Confidence { get; private init; }

    /// <summary>The time horizon over which this prediction is asserted.</summary>
    public PredictionHorizon Horizon { get; private init; }

    /// <summary>The model name that generated this prediction.</summary>
    public string ModelName { get; private init; }

    /// <summary>The model version that generated this prediction.</summary>
    public string ModelVersion { get; private init; }

    /// <summary>UTC timestamp when this prediction was generated.</summary>
    public DateTime GeneratedAtUtc { get; private init; }

    /// <summary>Whether a safety alert has been raised for this prediction.</summary>
    public bool AlertRaised { get; private set; }

    /// <summary>Whether this prediction has been acknowledged by a human.</summary>
    public bool IsAcknowledged { get; private set; }

    /// <summary>The actor who acknowledged this prediction. Null until acknowledged.</summary>
    public string? AcknowledgedByActorId { get; private set; }

    /// <summary>UTC timestamp when this prediction was acknowledged. Null until acknowledged.</summary>
    public DateTime? AcknowledgedAtUtc { get; private set; }

    /// <summary>Evidence items supporting this prediction. At least one required before publishing (Rule 4.6).</summary>
    public IReadOnlyList<PredictionEvidence> Evidence => _evidence.AsReadOnly();

    private SafetyPrediction(
        SafetyPredictionId id,
        PredictionSubject subject,
        RiskLevel riskLevel,
        ConfidenceScore confidence,
        PredictionHorizon horizon,
        string modelName,
        string modelVersion)
        : base(id)
    {
        Subject = subject;
        RiskLevel = riskLevel;
        Confidence = confidence;
        Horizon = horizon;
        ModelName = modelName;
        ModelVersion = modelVersion;
        GeneratedAtUtc = DateTime.UtcNow;
        AlertRaised = false;
        IsAcknowledged = false;
    }

    /// <summary>
    /// SAFETY-CRITICAL: Generates a new safety prediction.
    /// Predictions with confidence below the minimum threshold are created but will be blocked
    /// from automated actions by <see cref="ConfidenceScore.MeetsMinimumThreshold"/> checks (Rule 4.4, 4.5).
    /// Evidence must be added via <see cref="AddEvidence"/> before <see cref="Publish"/> is called.
    /// </summary>
    /// <param name="subject">The plant area, process, or equipment being assessed.</param>
    /// <param name="riskLevel">The assessed risk level.</param>
    /// <param name="confidence">The prediction confidence score.</param>
    /// <param name="horizon">The prediction time horizon.</param>
    /// <param name="modelName">The model that generated this prediction.</param>
    /// <param name="modelVersion">The model version.</param>
    /// <param name="correlationId">Correlation identifier for lineage.</param>
    /// <param name="causationId">Causation identifier from the triggering command.</param>
    /// <returns>A new <see cref="SafetyPrediction"/> not yet published.</returns>
    public static SafetyPrediction Generate(
        PredictionSubject subject,
        RiskLevel riskLevel,
        ConfidenceScore confidence,
        PredictionHorizon horizon,
        string modelName,
        string modelVersion,
        Guid correlationId,
        Guid causationId)
    {
        if (string.IsNullOrWhiteSpace(modelName))
            throw new ArgumentException("ModelName cannot be empty.", nameof(modelName));
        if (string.IsNullOrWhiteSpace(modelVersion))
            throw new ArgumentException("ModelVersion cannot be empty.", nameof(modelVersion));

        var id = SafetyPredictionId.New();
        return new SafetyPrediction(id, subject, riskLevel, confidence, horizon, modelName.Trim(), modelVersion.Trim());
    }

    /// <summary>
    /// Adds an evidence item to this prediction. Required for explainability (Safety Rule 4.6).
    /// Evidence cannot be added after the prediction is acknowledged.
    /// </summary>
    /// <param name="description">Human-readable explanation of this evidence.</param>
    /// <param name="featureName">The feature that contributed this evidence.</param>
    /// <param name="featureContributionWeight">SHAP weight in [-1.0, 1.0].</param>
    /// <param name="observedValue">The observed feature value (masked, no PII).</param>
    /// <param name="correlationId">Correlation identifier.</param>
    /// <param name="causationId">Causation identifier.</param>
    /// <exception cref="IntelligenceDomainException">Thrown when prediction is acknowledged or evidence limit reached.</exception>
    public void AddEvidence(
        string description,
        string featureName,
        double featureContributionWeight,
        string observedValue,
        Guid correlationId,
        Guid causationId)
    {
        if (IsAcknowledged)
            throw new IntelligenceDomainException(
                "PREDICTION_ACKNOWLEDGED", "Cannot add evidence to an acknowledged prediction.",
                correlationId, causationId);

        if (_evidence.Count >= IntelligenceConstants.MaxEvidencePerPrediction)
            throw new IntelligenceDomainException(
                "PREDICTION_EVIDENCE_LIMIT_EXCEEDED",
                $"A prediction cannot have more than {IntelligenceConstants.MaxEvidencePerPrediction} evidence items.",
                correlationId, causationId);

        _evidence.Add(PredictionEvidence.Create(Id, description, featureName, featureContributionWeight, observedValue));
    }

    /// <summary>
    /// SAFETY-CRITICAL: Publishes this prediction, raising a <see cref="SafetyPredictionGeneratedEvent"/>.
    /// Requires at least one evidence item (Rule 4.6). The event carries the confidence score so
    /// downstream consumers can enforce the threshold themselves.
    /// </summary>
    /// <param name="correlationId">Correlation identifier.</param>
    /// <param name="causationId">Causation identifier.</param>
    /// <exception cref="IntelligenceDomainException">Thrown when no evidence is present.</exception>
    public void Publish(Guid correlationId, Guid causationId)
    {
        if (_evidence.Count == 0)
            throw new IntelligenceDomainException(
                "PREDICTION_NO_EVIDENCE",
                "A safety prediction must have at least one evidence item before it can be published (Safety Rule 4.6).",
                correlationId, causationId);

        RaiseDomainEvent(new SafetyPredictionGeneratedEvent
        {
            AggregateId = Id.Value,
            CorrelationId = correlationId,
            CausationId = causationId,
            Subject = Subject.Value,
            RiskLevel = RiskLevel.Value,
            ConfidenceScore = Confidence.Value,
            MeetsMinimumThreshold = Confidence.MeetsMinimumThreshold,
            HorizonHours = Horizon.Hours,
            ModelName = ModelName,
            ModelVersion = ModelVersion,
            EvidenceCount = _evidence.Count
        });
    }

    /// <summary>
    /// SAFETY-CRITICAL: Raises a safety alert for this prediction. Must only be called after
    /// dual validation passes for High/Critical risk levels (enforced by <see cref="SafetyPredictionDomainService"/>).
    /// A prediction that does not meet the confidence threshold must never reach this method.
    /// </summary>
    /// <param name="correlationId">Correlation identifier.</param>
    /// <param name="causationId">Causation identifier.</param>
    /// <exception cref="IntelligenceDomainException">
    /// Thrown when confidence does not meet the minimum threshold, alert already raised,
    /// or prediction is already acknowledged.
    /// </exception>
    public void RaiseAlert(Guid correlationId, Guid causationId)
    {
        // Safety Rule 4.5 — fail safe, not fail open.
        if (!Confidence.MeetsMinimumThreshold)
            throw new IntelligenceDomainException(
                "PREDICTION_CONFIDENCE_BELOW_FLOOR",
                $"A safety alert cannot be raised for a prediction with confidence {Confidence.Value:F4} below the minimum threshold {IntelligenceConstants.MinimumConfidenceScore:F2} (Safety Rule 4.4).",
                correlationId, causationId);

        if (AlertRaised)
            throw new IntelligenceDomainException(
                "PREDICTION_ALERT_ALREADY_RAISED", "A safety alert has already been raised for this prediction.",
                correlationId, causationId);

        if (IsAcknowledged)
            throw new IntelligenceDomainException(
                "PREDICTION_ACKNOWLEDGED", "Cannot raise an alert on an acknowledged prediction.",
                correlationId, causationId);

        AlertRaised = true;

        RaiseDomainEvent(new SafetyAlertRaisedEvent
        {
            AggregateId = Id.Value,
            CorrelationId = correlationId,
            CausationId = causationId,
            Subject = Subject.Value,
            RiskLevel = RiskLevel.Value,
            ConfidenceScore = Confidence.Value,
            HorizonHours = Horizon.Hours,
            ModelName = ModelName
        });
    }

    /// <summary>
    /// Records a human acknowledgement of this prediction. Once acknowledged, the prediction is
    /// terminal — no automated process may undo this decision (Safety Rule 4.7).
    /// </summary>
    /// <param name="actorId">The human actor acknowledging the prediction.</param>
    /// <param name="correlationId">Correlation identifier.</param>
    /// <param name="causationId">Causation identifier.</param>
    /// <exception cref="IntelligenceDomainException">Thrown when already acknowledged.</exception>
    public void Acknowledge(string actorId, Guid correlationId, Guid causationId)
    {
        if (IsAcknowledged)
            throw new IntelligenceDomainException(
                "PREDICTION_ALREADY_ACKNOWLEDGED", "This prediction has already been acknowledged.",
                correlationId, causationId);

        IsAcknowledged = true;
        AcknowledgedByActorId = actorId;
        AcknowledgedAtUtc = DateTime.UtcNow;

        RaiseDomainEvent(new PredictionAcknowledgedEvent
        {
            AggregateId = Id.Value,
            CorrelationId = correlationId,
            CausationId = causationId,
            Subject = Subject.Value,
            RiskLevel = RiskLevel.Value,
            AcknowledgedByActorId = actorId,
            AcknowledgedAtUtc = AcknowledgedAtUtc.Value
        });
    }
}
