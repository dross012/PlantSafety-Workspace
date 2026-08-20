// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.FeatureEngineering
//  Layer:          Domain
//  File:           FeatureVersion.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    Value Object: version ("MAJOR.MINOR") of a published ML feature set. Ported
//                  (rebuilt, not copy-pasted) from the orphaned, dead
//                  Intelligence.Domain/FeatureEngineering/ValueObjects/FeatureVersion.cs onto this
//                  module's own PlantSafety.Domain.Common.Base.ValueObject record base instead of
//                  the orphan's local Intelligence.Domain.Shared.Abstractions.ValueObject
//                  duplicate. Backs FeatureEngineeringDomainService.Supersedes's version-ordinal
//                  comparison.
//                  Additive-only evolution.
// ============================================================================
using System.Text.RegularExpressions;

namespace PlantSafety.Domain.Intelligence.FeatureEngineering.ValueObjects;

/// <summary>Version of an ML feature set. Format: "MAJOR.MINOR" (e.g., "1.0").</summary>
public sealed record FeatureVersion : ValueObject
{
    private static readonly Regex VersionPattern = new(@"^\d+\.\d+$", RegexOptions.Compiled);

    /// <summary>The version string.</summary>
    public string Value { get; }

    /// <summary>Major version component.</summary>
    public int Major { get; }

    /// <summary>Minor version component.</summary>
    public int Minor { get; }

    private FeatureVersion(string value, int major, int minor)
    {
        Value = value;
        Major = major;
        Minor = minor;
    }

    /// <summary>Creates a <see cref="FeatureVersion"/> from a version string.</summary>
    /// <exception cref="ArgumentException">Thrown when value does not match MAJOR.MINOR format.</exception>
    public static FeatureVersion From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("FeatureVersion cannot be empty.", nameof(value));
        var trimmed = value.Trim();
        if (!VersionPattern.IsMatch(trimmed))
            throw new ArgumentException($"FeatureVersion must be in 'MAJOR.MINOR' format. Received: '{trimmed}'.", nameof(value));
        var parts = trimmed.Split('.');
        return new FeatureVersion(trimmed, int.Parse(parts[0]), int.Parse(parts[1]));
    }

    /// <summary>The initial version for a newly created feature set.</summary>
    public static FeatureVersion Initial => From("1.0");

    /// <summary>Returns <see langword="true"/> when this version is ordinally greater than <paramref name="other"/> (higher major, then minor).</summary>
    public bool Supersedes(FeatureVersion other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (Major != other.Major) return Major > other.Major;
        return Minor > other.Minor;
    }

    /// <inheritdoc/>
    public override string ToString() => Value;
}
