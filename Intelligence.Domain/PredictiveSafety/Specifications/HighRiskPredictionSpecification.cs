/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               HighRiskPredictionSpecification.cs
    DESCRIPTION:        SAFETY-CRITICAL Specification that identifies predictions requiring immediate human
                        escalation. Satisfied when a prediction is High or Critical risk, meets the confidence
                        floor, has not been acknowledged, and has raised (or is eligible to raise) an alert.
                        Used by application-layer query handlers to surface urgent items. (Rule 2.9)

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only. Severity threshold may only be lowered (expanding coverage), never raised.
==========================================================================================================================*/

using Intelligence.Domain.Shared.Interfaces;
using Intelligence.Domain.PredictiveSafety.Aggregates;
using Intelligence.Domain.PredictiveSafety.ValueObjects;

namespace Intelligence.Domain.PredictiveSafety.Specifications;

/// <summary>
/// SAFETY-CRITICAL: Satisfied when a <see cref="SafetyPrediction"/> is a high-priority item requiring
/// immediate human review: High or Critical risk, above confidence floor, and not yet acknowledged.
/// </summary>
public sealed class HighRiskPredictionSpecification : ISpecification<SafetyPrediction>
{
    /// <inheritdoc />
    public string Description =>
        "SafetyPrediction must be High or Critical risk, meet the minimum confidence threshold, and not yet be acknowledged.";

    /// <inheritdoc />
    public bool IsSatisfiedBy(SafetyPrediction entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return entity.RiskLevel.IsAtLeastAsSevereAs(RiskLevel.High)
               && entity.Confidence.MeetsMinimumThreshold
               && !entity.IsAcknowledged;
    }
}
