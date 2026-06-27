/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               PredictionEvidence.cs
    DESCRIPTION:        SAFETY-CRITICAL Entity representing a single piece of evidence supporting a safety
                        prediction. Evidence is mandatory for explainability (Safety Rule 4.6). Each evidence
                        item must have a human-readable description and a feature contribution weight.
                        The aggregate enforces the maximum evidence count per IntelligenceConstants.

    INVARIANTS:
        - Description must not be empty (explainability requirement, Safety Rule 4.6).
        - FeatureContributionWeight must be in [-1.0, 1.0] (SHAP-compatible).
        - Evidence is immutable once created.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using Intelligence.Domain.Shared.Abstractions;
using Intelligence.Domain.PredictiveSafety.ValueObjects;

namespace Intelligence.Domain.PredictiveSafety.Entities;

/// <summary>
/// SAFETY-CRITICAL: A single piece of evidence supporting a <see cref="Intelligence.Domain.PredictiveSafety.Aggregates.SafetyPrediction"/>.
/// All safety predictions must carry at least one evidence item (Safety Rule 4.6 — explainability required).
/// </summary>
public sealed class PredictionEvidence : Entity<Guid>
{
    /// <summary>The prediction this evidence supports.</summary>
    public SafetyPredictionId PredictionId { get; private init; }

    /// <summary>
    /// Human-readable explanation of why this evidence supports the prediction.
    /// Required — predictions without explanations violate Safety Rule 4.6.
    /// </summary>
    public string Description { get; private init; }

    /// <summary>The name of the feature that produced this evidence.</summary>
    public string FeatureName { get; private init; }

    /// <summary>
    /// The SHAP-compatible contribution weight of this feature to the prediction.
    /// Range: [-1.0, 1.0]. Positive values increase predicted risk; negative values reduce it.
    /// </summary>
    public double FeatureContributionWeight { get; private init; }

    /// <summary>The observed value of the feature at prediction time (masked — no raw PII).</summary>
    public string ObservedValue { get; private init; }

    private PredictionEvidence(
        Guid id,
        SafetyPredictionId predictionId,
        string description,
        string featureName,
        double featureContributionWeight,
        string observedValue)
        : base(id)
    {
        PredictionId = predictionId;
        Description = description;
        FeatureName = featureName;
        FeatureContributionWeight = featureContributionWeight;
        ObservedValue = observedValue;
    }

    /// <summary>
    /// Creates a new <see cref="PredictionEvidence"/> item for a safety prediction.
    /// </summary>
    /// <param name="predictionId">The owning prediction.</param>
    /// <param name="description">Human-readable explanation. Required for explainability (Safety Rule 4.6).</param>
    /// <param name="featureName">The feature that produced this evidence.</param>
    /// <param name="featureContributionWeight">SHAP weight in [-1.0, 1.0].</param>
    /// <param name="observedValue">The observed feature value (masked, no PII).</param>
    /// <returns>A new immutable <see cref="PredictionEvidence"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when description or featureName is empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when featureContributionWeight is outside [-1.0, 1.0].</exception>
    public static PredictionEvidence Create(
        SafetyPredictionId predictionId,
        string description,
        string featureName,
        double featureContributionWeight,
        string observedValue)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException(
                "PredictionEvidence description is required for explainability (Safety Rule 4.6).", nameof(description));
        if (string.IsNullOrWhiteSpace(featureName))
            throw new ArgumentException("FeatureName cannot be empty.", nameof(featureName));
        if (featureContributionWeight is < -1.0 or > 1.0)
            throw new ArgumentOutOfRangeException(nameof(featureContributionWeight),
                $"FeatureContributionWeight must be in [-1.0, 1.0]. Received: {featureContributionWeight}.");

        return new PredictionEvidence(
            Guid.NewGuid(),
            predictionId,
            description.Trim(),
            featureName.Trim(),
            featureContributionWeight,
            (observedValue ?? string.Empty).Trim());
    }
}
