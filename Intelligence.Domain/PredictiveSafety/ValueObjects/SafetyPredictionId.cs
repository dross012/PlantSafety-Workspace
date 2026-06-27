/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               SafetyPredictionId.cs
    DESCRIPTION:        Strongly-typed identifier for the SafetyPrediction aggregate root. (Rule 1.2)

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace Intelligence.Domain.PredictiveSafety.ValueObjects;

/// <summary>
/// Strongly-typed identifier for a <see cref="Intelligence.Domain.PredictiveSafety.Aggregates.SafetyPrediction"/>.
/// </summary>
public readonly record struct SafetyPredictionId(Guid Value)
{
    /// <summary>Creates a new <see cref="SafetyPredictionId"/> with a newly generated identifier.</summary>
    public static SafetyPredictionId New() => new(Guid.NewGuid());

    /// <summary>Creates a <see cref="SafetyPredictionId"/> from an existing <see cref="Guid"/>.</summary>
    /// <exception cref="ArgumentException">Thrown when value is <see cref="Guid.Empty"/>.</exception>
    public static SafetyPredictionId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SafetyPredictionId cannot be empty.", nameof(value));
        return new(value);
    }

    /// <inheritdoc />
    public override string ToString() => Value.ToString("N");
}
