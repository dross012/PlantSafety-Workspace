/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               FeatureVersion.cs
    DESCRIPTION:        Value Object: version of a feature set. Format MAJOR.MINOR. Additive-only.
    SCHEMA VERSION:     2026.1  |  EVOLUTION: Additive-only.
==========================================================================================================================*/

using Intelligence.Domain.Shared.Abstractions;

namespace Intelligence.Domain.FeatureEngineering.ValueObjects;

/// <summary>Version of an ML feature set. Format: "MAJOR.MINOR" (e.g., "1.0").</summary>
public sealed class FeatureVersion : ValueObject
{
    private static readonly System.Text.RegularExpressions.Regex VersionPattern =
        new(@"^\d+\.\d+$", System.Text.RegularExpressions.RegexOptions.Compiled);

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

    /// <summary>The initial version for a newly created feature.</summary>
    public static FeatureVersion Initial => From("1.0");

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }

    /// <inheritdoc />
    public override string ToString() => Value;
}
