/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               FindingConfidence.cs
    DESCRIPTION:        Value Object representing the confidence level of an investigation finding.
                        Range: [0.0, 1.0]. Threshold gate per IntelligenceConstants.MinimumConfidenceScore.
                        (Rule 2.8, Safety Rule 4.4)

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using Intelligence.Domain.Shared.Abstractions;
using Intelligence.Domain.Shared.Constants;

namespace Intelligence.Domain.AIInvestigations.ValueObjects;

/// <summary>Confidence level of an investigation finding. Range: [0.0, 1.0].</summary>
public sealed class FindingConfidence : ValueObject
{
    /// <summary>The numeric confidence value in [0.0, 1.0].</summary>
    public double Value { get; }

    /// <summary>Whether this confidence meets the minimum threshold (Safety Rule 4.4).</summary>
    public bool MeetsMinimumThreshold => Value >= IntelligenceConstants.MinimumConfidenceScore;

    private FindingConfidence(double value) => Value = value;

    /// <summary>Creates a <see cref="FindingConfidence"/> from a double.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when value is outside [0.0, 1.0].</exception>
    public static FindingConfidence From(double value)
    {
        if (value is < 0.0 or > 1.0)
            throw new ArgumentOutOfRangeException(nameof(value),
                $"FindingConfidence must be between 0.0 and 1.0. Received: {value}.");
        return new FindingConfidence(value);
    }

    /// <summary>Attempts to create a <see cref="FindingConfidence"/>.</summary>
    public static bool TryParse(double value, out FindingConfidence? result)
    {
        try { result = From(value); return true; }
        catch { result = null; return false; }
    }

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }

    /// <inheritdoc />
    public override string ToString() => $"{Value:F4}";
}
