// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Extensions
//  Layer:          SharedKernel
//  File:           StringExtensions.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Pure, deterministic string extension methods.
//                  SIEM-safe: no logging of raw values. Additive-only evolution.
// ============================================================================
namespace PlantSafety.SharedKernel.Extensions;

/// <summary>Pure, deterministic string extension methods.</summary>
public static class StringExtensions
{
    /// <summary>Returns true if the value is null or consists only of white-space characters.</summary>
    public static bool IsNullOrWhiteSpace(this string? value) => string.IsNullOrWhiteSpace(value);

    /// <summary>Trims the value and returns null if the result is empty.</summary>
    public static string? TrimToNull(this string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }

    /// <summary>Truncates the string to at most <paramref name="maxLength"/> characters.</summary>
    public static string Truncate(this string value, int maxLength)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxLength);
        return value.Length <= maxLength ? value : value[..maxLength];
    }

    /// <summary>Converts a PascalCase or camelCase string to snake_case.</summary>
    public static string ToSnakeCase(this string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        var result = new System.Text.StringBuilder();
        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];
            if (char.IsUpper(c) && i > 0) result.Append('_');
            result.Append(char.ToLowerInvariant(c));
        }
        return result.ToString();
    }

    /// <summary>
    /// Masks the middle portion of a string, leaving the first and last
    /// <paramref name="visibleChars"/> characters visible.
    /// Used for SIEM-safe logging of partially sensitive values.
    /// </summary>
    public static string Mask(this string value, int visibleChars = 2)
    {
        if (value.Length <= visibleChars * 2) return new string('*', value.Length);
        return value[..visibleChars] + new string('*', value.Length - visibleChars * 2) + value[^visibleChars..];
    }
}
