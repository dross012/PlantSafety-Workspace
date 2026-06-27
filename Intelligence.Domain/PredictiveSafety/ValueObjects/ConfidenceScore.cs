/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               ConfidenceScore.cs
    DESCRIPTION:        SAFETY-CRITICAL Value Object representing the confidence level of a safety prediction.
                        Range: [0.0, 1.0]. The minimum floor for automated safety actions is
                        IntelligenceConstants.MinimumConfidenceScore (0.70 — Safety Rule 4.4).

                        Predictions with a score below the floor MUST NOT drive automated safety actions.
                        The floor may only be raised, never lowered, by configuration.
                        This constraint is enforced here, at the aggregate boundary, and in domain services.
                        Fail safe, not fail open (Safety Rule 4.5).

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only. MinimumConfidenceScore may only be raised, never lowered.
==========================================================================================================================*/

using Intelligence.Domain.Shared.Abstractions;
using Intelligence.Domain.Shared.Constants;

namespace Intelligence.Domain.PredictiveSafety.ValueObjects;

/// <summary>
/// SAFETY-CRITICAL: Confidence level of a safety prediction. Range: [0.0, 1.0].
/// Predictions with <see cref="MeetsMinimumThreshold"/> false must not trigger automated safety actions.
/// </summary>
public sealed class ConfidenceScore : ValueObject
{
    /// <summary>The numeric confidence score in [0.0, 1.0].</summary>
    public double Value { get; }

    /// <summary>
    /// SAFETY-CRITICAL: Whether this confidence score meets the minimum threshold for automated
    /// safety actions (Safety Rule 4.4). When false, the prediction must be blocked and routed
    /// for human review — it must never be passed through as a safety recommendation.
    /// </summary>
    public bool MeetsMinimumThreshold => Value >= IntelligenceConstants.MinimumConfidenceScore;

    private ConfidenceScore(double value) => Value = value;

    /// <summary>
    /// Creates a <see cref="ConfidenceScore"/> from a numeric score.
    /// </summary>
    /// <param name="value">A confidence value in [0.0, 1.0].</param>
    /// <returns>A validated <see cref="ConfidenceScore"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when value is outside [0.0, 1.0].</exception>
    public static ConfidenceScore From(double value)
    {
        if (value is < 0.0 or > 1.0)
            throw new ArgumentOutOfRangeException(nameof(value),
                $"ConfidenceScore must be between 0.0 and 1.0. Received: {value}.");
        return new ConfidenceScore(value);
    }

    /// <summary>Attempts to create a <see cref="ConfidenceScore"/> from a double.</summary>
    public static bool TryParse(double value, out ConfidenceScore? result)
    {
        try { result = From(value); return true; }
        catch { result = null; return false; }
    }

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <inheritdoc />
    public override string ToString() => $"{Value:F4}";
}
