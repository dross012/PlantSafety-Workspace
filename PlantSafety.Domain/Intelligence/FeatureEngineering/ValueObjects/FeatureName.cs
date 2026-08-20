// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.FeatureEngineering
//  Layer:          Domain
//  File:           FeatureName.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    Value Object: name of an ML feature, normalized to lowercase snake_case. Ported
//                  (rebuilt, not copy-pasted) from the orphaned, dead
//                  Intelligence.Domain/FeatureEngineering/ValueObjects/FeatureName.cs onto this
//                  module's own PlantSafety.Domain.Common.Base.ValueObject record base instead of
//                  the orphan's local Intelligence.Domain.Shared.Abstractions.ValueObject
//                  duplicate. The orphan's IntelligenceConstants.MaxFeatureNameLength is inlined
//                  here as MaxLength since no shared Intelligence-module constants holder exists
//                  among this module's sibling sub-folders to extend.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Intelligence.FeatureEngineering.ValueObjects;

/// <summary>Name of an ML feature. Normalized to lowercase with underscores.</summary>
public sealed record FeatureName : ValueObject
{
    /// <summary>Maximum permitted length of a normalized feature name.</summary>
    public const int MaxLength = 200;

    /// <summary>The normalized feature name.</summary>
    public string Value { get; }

    private FeatureName(string value) => Value = value;

    /// <summary>Creates a <see cref="FeatureName"/> from a raw name string.</summary>
    /// <exception cref="ArgumentException">Thrown when empty or exceeds <see cref="MaxLength"/>.</exception>
    public static FeatureName From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("FeatureName cannot be empty.", nameof(value));

        var normalized = value.Trim().ToLowerInvariant().Replace(' ', '_');

        if (normalized.Length > MaxLength)
            throw new ArgumentException(
                $"FeatureName must not exceed {MaxLength} characters. Received: {normalized.Length}.",
                nameof(value));

        return new FeatureName(normalized);
    }

    /// <inheritdoc/>
    public override string ToString() => Value;
}
