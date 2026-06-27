/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               InvestigationId.cs
    DESCRIPTION:        Strongly-typed identifier for the Investigation aggregate root. (Rule 1.2)

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace Intelligence.Domain.AIInvestigations.ValueObjects;

/// <summary>Strongly-typed identifier for an <see cref="Intelligence.Domain.AIInvestigations.Aggregates.Investigation"/>.</summary>
public readonly record struct InvestigationId(Guid Value)
{
    /// <summary>Creates a new <see cref="InvestigationId"/> with a newly generated identifier.</summary>
    public static InvestigationId New() => new(Guid.NewGuid());

    /// <summary>Creates an <see cref="InvestigationId"/> from an existing <see cref="Guid"/>.</summary>
    /// <exception cref="ArgumentException">Thrown when value is <see cref="Guid.Empty"/>.</exception>
    public static InvestigationId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("InvestigationId cannot be empty.", nameof(value));
        return new(value);
    }

    /// <inheritdoc />
    public override string ToString() => Value.ToString("N");
}
