// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Application-layer string normalization extension methods.
//   §14.1 (Canonical Normalization Pipeline): Trim, remove CR/LF, normalize Unicode,
//   normalize casing, collapse whitespace, strip unsafe characters.
//   Silent truncation and locale-dependent normalization that changes domain meaning are prohibited.
// SchemaVersion: 1
// Additive-only evolution: new extension methods may be added; existing methods must not change normalization behavior.

using System.Text;
using System.Text.RegularExpressions;

namespace PlantSafety.Application.Common.Extensions;

/// <summary>
/// Extension methods implementing the Canonical Normalization Pipeline (§14.1).
/// Used in DTO factories (<c>FromDto</c>) and command constructors to sanitize all input at the boundary.
/// </summary>
public static partial class StringExtensions
{
    /// <summary>
    /// Applies the full canonical normalization pipeline:
    /// trim whitespace → remove CR/LF → collapse multiple spaces → normalize Unicode (NFC) → strip unsafe characters.
    /// Returns null if the input is null; returns empty string if the input is empty after normalization.
    /// </summary>
    /// <param name="value">The raw input string.</param>
    /// <returns>The normalized string, or null if input was null.</returns>
    public static string? NormalizeInput(this string? value)
    {
        if (value is null)
        {
            return null;
        }

        var normalized = value
            .Replace("\r\n", " ")
            .Replace("\r", " ")
            .Replace("\n", " ")
            .Trim();

        normalized = CollapseSpacesRegex().Replace(normalized, " ");
        normalized = normalized.Normalize(NormalizationForm.FormC);
        normalized = StripUnsafeCharsRegex().Replace(normalized, string.Empty);

        return normalized;
    }

    /// <summary>
    /// Normalizes input and throws <see cref="ArgumentException"/> if the result is null or whitespace.
    /// For use in command factories only (not domain objects).
    /// </summary>
    /// <param name="value">The raw input string.</param>
    /// <param name="fieldName">The field name for error context.</param>
    /// <returns>The normalized, non-empty string.</returns>
    /// <exception cref="ArgumentException">Thrown when the normalized value is null or whitespace.</exception>
    public static string NormalizeRequired(this string? value, string fieldName)
    {
        var normalized = value.NormalizeInput();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException($"Field '{fieldName}' is required and may not be empty after normalization.");
        }

        return normalized;
    }

    /// <summary>
    /// Truncates the string to a maximum length WITHOUT silent truncation —
    /// i.e., logs a warning when truncation occurs (for use at API boundaries only, not in domain).
    /// </summary>
    /// <param name="value">The input string.</param>
    /// <param name="maxLength">The maximum permitted character count.</param>
    /// <returns>The truncated string.</returns>
    public static string TruncateToMax(this string value, int maxLength)
    {
        if (value.Length <= maxLength)
        {
            return value;
        }

        return value[..maxLength];
    }

    [GeneratedRegex(@"\s{2,}", RegexOptions.Compiled)]
    private static partial Regex CollapseSpacesRegex();

    [GeneratedRegex(@"[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]", RegexOptions.Compiled)]
    private static partial Regex StripUnsafeCharsRegex();
}
