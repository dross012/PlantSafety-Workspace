/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               SchemaVersion.cs
    DESCRIPTION:        Value Object: schema version of Data Lake records. Format "YYYY.N". Additive-only.
    SCHEMA VERSION:     2026.1  |  EVOLUTION: Additive-only.
==========================================================================================================================*/

using Intelligence.Domain.Shared.Abstractions;
using Intelligence.Domain.Shared.Constants;

namespace Intelligence.Domain.DataLake.ValueObjects;

/// <summary>Schema version of a Data Lake record. Format: "YYYY.N" (e.g., "2026.1").</summary>
public sealed class SchemaVersion : ValueObject
{
    private static readonly System.Text.RegularExpressions.Regex VersionPattern =
        new(@"^\d{4}\.\d+$", System.Text.RegularExpressions.RegexOptions.Compiled);

    /// <summary>The version string (e.g., "2026.1").</summary>
    public string Value { get; }

    private SchemaVersion(string value) => Value = value;

    /// <summary>Creates a <see cref="SchemaVersion"/> from a version string.</summary>
    /// <exception cref="ArgumentException">Thrown when value does not match YYYY.N format.</exception>
    public static SchemaVersion From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("SchemaVersion cannot be empty.", nameof(value));
        var trimmed = value.Trim();
        if (!VersionPattern.IsMatch(trimmed))
            throw new ArgumentException(
                $"SchemaVersion must be in 'YYYY.N' format (e.g., '{IntelligenceConstants.DefaultSchemaVersion}'). Received: '{trimmed}'.",
                nameof(value));
        return new SchemaVersion(trimmed);
    }

    /// <summary>The default schema version per <see cref="IntelligenceConstants.DefaultSchemaVersion"/>.</summary>
    public static SchemaVersion Default => From(IntelligenceConstants.DefaultSchemaVersion);

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }

    /// <inheritdoc />
    public override string ToString() => Value;
}
