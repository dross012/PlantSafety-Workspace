/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               PredictionSubject.cs
    DESCRIPTION:        Value Object representing the subject of a safety prediction — the plant area,
                        process, or equipment being assessed. Normalized at creation. Max length enforced
                        by IntelligenceConstants.MaxPredictionSubjectLength. (Rule 2.8)

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using Intelligence.Domain.Shared.Abstractions;
using Intelligence.Domain.Shared.Constants;

namespace Intelligence.Domain.PredictiveSafety.ValueObjects;

/// <summary>
/// The subject of a safety prediction: the plant area, process, or equipment being assessed.
/// Normalized (trimmed, CR/LF stripped). Max length: <see cref="IntelligenceConstants.MaxPredictionSubjectLength"/>.
/// </summary>
public sealed class PredictionSubject : ValueObject
{
    /// <summary>The normalized prediction subject string.</summary>
    public string Value { get; }

    private PredictionSubject(string value) => Value = value;

    /// <summary>
    /// Creates a <see cref="PredictionSubject"/> from a raw subject string.
    /// </summary>
    /// <param name="value">The subject. Whitespace trimmed, CR/LF stripped.</param>
    /// <returns>A normalized <see cref="PredictionSubject"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when value is empty or exceeds max length.</exception>
    public static PredictionSubject From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("PredictionSubject cannot be empty.", nameof(value));

        var normalized = value.Trim()
            .Replace("\r\n", " ", StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal)
            .Replace("\r", " ", StringComparison.Ordinal);

        if (normalized.Length > IntelligenceConstants.MaxPredictionSubjectLength)
            throw new ArgumentException(
                $"PredictionSubject must not exceed {IntelligenceConstants.MaxPredictionSubjectLength} characters. Received length: {normalized.Length}.",
                nameof(value));

        return new PredictionSubject(normalized);
    }

    /// <summary>Attempts to create a <see cref="PredictionSubject"/>.</summary>
    public static bool TryParse(string value, out PredictionSubject? result)
    {
        try { result = From(value); return true; }
        catch { result = null; return false; }
    }

    /// <summary>Implicit conversion to string for use in display contexts.</summary>
    public static implicit operator string(PredictionSubject s) => s.Value;

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <inheritdoc />
    public override string ToString() => Value;
}
