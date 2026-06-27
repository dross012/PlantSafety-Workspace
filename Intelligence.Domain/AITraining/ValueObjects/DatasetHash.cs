/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               DatasetHash.cs
    DESCRIPTION:        Value Object: SHA-256 hash of a training dataset. Exactly 64 lowercase hex chars.
    SCHEMA VERSION:     2026.1  |  EVOLUTION: Additive-only.
==========================================================================================================================*/

using Intelligence.Domain.Shared.Abstractions;
using Intelligence.Domain.Shared.Constants;

namespace Intelligence.Domain.AITraining.ValueObjects;

/// <summary>SHA-256 hash of a training dataset. Exactly 64 lowercase hexadecimal characters.</summary>
public sealed class DatasetHash : ValueObject
{
    /// <summary>The normalized lowercase hex hash string.</summary>
    public string Value { get; }

    private DatasetHash(string value) => Value = value;

    /// <summary>Creates a <see cref="DatasetHash"/> from a hex string.</summary>
    /// <exception cref="ArgumentException">Thrown when value is not a valid 64-character hex string.</exception>
    public static DatasetHash From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("DatasetHash cannot be empty.", nameof(value));

        var normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length != IntelligenceConstants.DatasetHashHexLength)
            throw new ArgumentException(
                $"DatasetHash must be exactly {IntelligenceConstants.DatasetHashHexLength} hex characters. Received length: {normalized.Length}.",
                nameof(value));

        if (!normalized.All(c => c is >= '0' and <= '9' or >= 'a' and <= 'f'))
            throw new ArgumentException("DatasetHash must contain only hexadecimal characters (0-9, a-f).", nameof(value));

        return new DatasetHash(normalized);
    }

    /// <summary>Attempts to create a <see cref="DatasetHash"/>.</summary>
    public static bool TryParse(string value, out DatasetHash? result)
    {
        try { result = From(value); return true; }
        catch { result = null; return false; }
    }

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }

    /// <inheritdoc />
    public override string ToString() => Value;
}
