/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               TrainingJobId.cs
    SCHEMA VERSION:     2026.1  |  EVOLUTION: Additive-only.
==========================================================================================================================*/

namespace Intelligence.Domain.AITraining.ValueObjects;

/// <summary>Strongly-typed identifier for a <see cref="Intelligence.Domain.AITraining.Aggregates.TrainingJob"/>.</summary>
public readonly record struct TrainingJobId(Guid Value)
{
    /// <summary>Creates a new <see cref="TrainingJobId"/> with a newly generated identifier.</summary>
    public static TrainingJobId New() => new(Guid.NewGuid());

    /// <summary>Creates a <see cref="TrainingJobId"/> from an existing <see cref="Guid"/>.</summary>
    /// <exception cref="ArgumentException">Thrown when value is <see cref="Guid.Empty"/>.</exception>
    public static TrainingJobId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("TrainingJobId cannot be empty.", nameof(value));
        return new(value);
    }

    /// <inheritdoc />
    public override string ToString() => Value.ToString("N");
}
