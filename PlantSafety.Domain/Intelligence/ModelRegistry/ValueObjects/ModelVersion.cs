// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.ModelRegistry
//  Layer:          Domain
//  File:           ModelVersion.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
//  SchemaVersion:  2026.1
//  Description:    Value Object: semantic version ("MAJOR.MINOR.PATCH") of a registered ML model.
//                  Ported (rebuilt, not copy-pasted) from the orphaned, dead
//                  Intelligence.Domain/ModelRegistry/ValueObjects/ModelVersion.cs onto this module's
//                  own PlantSafety.Domain.Common.Base.ValueObject record base instead of the
//                  orphan's local Intelligence.Domain.Shared.Abstractions.ValueObject duplicate.
//                  Backs ModelRegistryDomainService.SupersedesCurrentDeployment's SemVer-ordinal
//                  comparison.
//                  Additive-only evolution.
// ============================================================================
using System.Text.RegularExpressions;

namespace PlantSafety.Domain.Intelligence.ModelRegistry.ValueObjects;

/// <summary>Semantic version of a registered ML model. Format: "MAJOR.MINOR.PATCH" (e.g., "1.0.0").</summary>
public sealed record ModelVersion : ValueObject
{
    private static readonly Regex SemVerPattern = new(@"^\d+\.\d+\.\d+$", RegexOptions.Compiled);

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

    /// <summary>Returns <see langword="true"/> when this version is ordinally greater than <paramref name="other"/>.</summary>
    public bool IsGreaterThan(ModelVersion other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (Major != other.Major) return Major > other.Major;
        if (Minor != other.Minor) return Minor > other.Minor;
        return Patch > other.Patch;
    }

    /// <inheritdoc/>
    public override string ToString() => Value;
}
