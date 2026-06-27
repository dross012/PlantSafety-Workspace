/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               FeatureName.cs
    DESCRIPTION:        Value Object: name of an ML feature. Normalized to lowercase snake_case.
    SCHEMA VERSION:     2026.1  |  EVOLUTION: Additive-only.
==========================================================================================================================*/

using Intelligence.Domain.Shared.Abstractions;
using Intelligence.Domain.Shared.Constants;

namespace Intelligence.Domain.FeatureEngineering.ValueObjects;

/// <summary>Name of an ML feature. Normalized to lowercase with underscores. Max: <see cref="IntelligenceConstants.MaxFeatureNameLength"/>.</summary>
public sealed class FeatureName : ValueObject
{
    /// <summary>The normalized feature name.</summary>
    public string Value { get; }

    private FeatureName(string value) => Value = value;

    /// <summary>Creates a <see cref="FeatureName"/> from a raw name string.</summary>
    /// <exception cref="ArgumentException">Thrown when empty or exceeds max length.</exception>
    public static FeatureName From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("FeatureName cannot be empty.", nameof(value));

        var normalized = value.Trim().ToLowerInvariant().Replace(' ', '_');

        if (normalized.Length > IntelligenceConstants.MaxFeatureNameLength)
            throw new ArgumentException(
                $"FeatureName must not exceed {IntelligenceConstants.MaxFeatureNameLength} characters. Received: {normalized.Length}.",
                nameof(value));

        return new FeatureName(normalized);
    }

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }

    /// <inheritdoc />
    public override string ToString() => Value;
}
