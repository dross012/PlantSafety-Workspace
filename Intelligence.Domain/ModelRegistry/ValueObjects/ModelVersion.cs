/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               ModelVersion.cs
    DESCRIPTION:        Value Object: semantic version of a registered ML model. Format MAJOR.MINOR.PATCH.
    SCHEMA VERSION:     2026.1  |  EVOLUTION: Additive-only.
==========================================================================================================================*/

using Intelligence.Domain.Shared.Abstractions;

namespace Intelligence.Domain.ModelRegistry.ValueObjects;

/// <summary>Semantic version of a registered ML model. Format: "MAJOR.MINOR.PATCH" (e.g., "1.0.0").</summary>
public sealed class ModelVersion : ValueObject
{
    private static readonly System.Text.RegularExpressions.Regex SemVerPattern =
        new(@"^\d+\.\d+\.\d+$", System.Text.RegularExpressions.RegexOptions.Compiled);

    /// <summary>The version string.</summary>
    public string Value { get; }

    /// <summary>Major version component.</summary>
    public int Major { get; }

    /// <summary>Minor version component.</summary>
    public int Minor { get; }

    /// <summary>Patch version component.</summary>
    public int Patch { get; }

    private ModelVersion(string value, int major, int minor, int patch)
    {
        Value = value;
        Major = major;
        Minor = minor;
        Patch = patch;
    }

    /// <summary>Creates a <see cref="ModelVersion"/> from a SemVer string.</summary>
    /// <exception cref="ArgumentException">Thrown when value does not match MAJOR.MINOR.PATCH format.</exception>
    public static ModelVersion From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("ModelVersion cannot be empty.", nameof(value));
        var trimmed = value.Trim();
        if (!SemVerPattern.IsMatch(trimmed))
            throw new ArgumentException($"ModelVersion must be in 'MAJOR.MINOR.PATCH' format. Received: '{trimmed}'.", nameof(value));
        var parts = trimmed.Split('.');
        return new ModelVersion(trimmed, int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]));
    }

    /// <summary>Attempts to create a <see cref="ModelVersion"/>.</summary>
    public static bool TryParse(string value, out ModelVersion? result)
    {
        try { result = From(value); return true; }
        catch { result = null; return false; }
    }

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }

    /// <inheritdoc />
    public override string ToString() => Value;
}
