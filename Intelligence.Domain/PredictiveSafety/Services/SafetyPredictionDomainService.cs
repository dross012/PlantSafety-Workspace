/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               SafetyPredictionDomainService.cs
    DESCRIPTION:        SAFETY-CRITICAL Domain service for predictive safety business rules. Enforces dual
                        validation for High/Critical risk predictions and provides stateless safety gate
                        evaluation. All safety rules from 4.4–4.7 are enforced here and at the aggregate.

                        DUAL VALIDATION (Safety Rule 4.4):
                          High and Critical risk predictions require two independent confidence assessments
                          before a safety alert is raised. The domain service compares a primary and a
                          secondary prediction against each other and verifies both meet the threshold.

                        This service accepts only domain types — no I/O, no external dependencies. (Rule 2.4)

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only. Safety rules may only be tightened.
==========================================================================================================================*/

using Intelligence.Domain.PredictiveSafety.Aggregates;
using Intelligence.Domain.PredictiveSafety.ValueObjects;
using Intelligence.Domain.Shared.Constants;

namespace Intelligence.Domain.PredictiveSafety.Services;

/// <summary>
/// SAFETY-CRITICAL: Domain service for predictive safety business rules. Evaluates confidence gates,
/// dual validation requirements, and alert eligibility. Stateless — no I/O.
/// </summary>
public sealed class SafetyPredictionDomainService
{
    /// <summary>
    /// SAFETY-CRITICAL: Evaluates whether a safety prediction is eligible for an automated safety alert.
    /// Returns false (fail safe) for any of:
    ///   - Confidence below the minimum threshold (Rule 4.4)
    ///   - High/Critical risk prediction that has not passed dual validation (Rule 4.4)
    ///   - Prediction already acknowledged by a human (Rule 4.7)
    ///   - Alert already raised
    /// </summary>
    /// <param name="prediction">The prediction to evaluate.</param>
    /// <param name="dualValidationPassed">
    /// Whether a second independent validation pass has been completed for this prediction.
    /// Must be <see langword="true"/> for <see cref="RiskLevel.High"/> and <see cref="RiskLevel.Critical"/> predictions.
    /// </param>
    /// <returns>
    /// <see langword="true"/> only when all safety gates pass and an alert is permitted.
    /// Returns <see langword="false"/> (fail safe) in all uncertain or invalid states.
    /// </returns>
    public bool IsEligibleForAlert(SafetyPrediction prediction, bool dualValidationPassed)
    {
        ArgumentNullException.ThrowIfNull(prediction);

        if (!prediction.Confidence.MeetsMinimumThreshold) return false;
        if (prediction.IsAcknowledged) return false;
        if (prediction.AlertRaised) return false;
        if (prediction.RiskLevel.RequiresDualValidation && !dualValidationPassed) return false;

        return true;
    }

    /// <summary>
    /// SAFETY-CRITICAL: Validates dual-validation consistency between a primary and secondary
    /// safety prediction for the same subject. Both predictions must:
    ///   - Meet the minimum confidence threshold (Rule 4.4)
    ///   - Assess the same subject
    ///   - Assess the same or higher risk level from the secondary
    /// </summary>
    /// <param name="primary">The primary prediction.</param>
    /// <param name="secondary">The independently-generated secondary prediction.</param>
    /// <returns>
    /// <see langword="true"/> when both predictions agree and validate each other.
    /// Returns <see langword="false"/> (fail safe) when they do not converge.
    /// </returns>
    public bool PassesDualValidation(SafetyPrediction primary, SafetyPrediction secondary)
    {
        ArgumentNullException.ThrowIfNull(primary);
        ArgumentNullException.ThrowIfNull(secondary);

        if (!primary.Confidence.MeetsMinimumThreshold) return false;
        if (!secondary.Confidence.MeetsMinimumThreshold) return false;

        if (!string.Equals(primary.Subject.Value, secondary.Subject.Value, StringComparison.OrdinalIgnoreCase))
            return false;

        return secondary.RiskLevel.IsAtLeastAsSevereAs(primary.RiskLevel);
    }

    /// <summary>
    /// Determines whether a prediction's confidence score meets the minimum operational threshold.
    /// Centralised here to allow application-layer callers to gate before calling domain methods.
    /// </summary>
    /// <param name="score">The confidence score to evaluate.</param>
    /// <returns><see langword="true"/> when the score meets or exceeds the floor.</returns>
    public bool MeetsConfidenceFloor(ConfidenceScore score)
    {
        ArgumentNullException.ThrowIfNull(score);
        return score.Value >= IntelligenceConstants.MinimumConfidenceScore;
    }
}
