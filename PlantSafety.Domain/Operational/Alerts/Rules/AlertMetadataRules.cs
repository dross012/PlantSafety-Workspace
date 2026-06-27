// ============================================================================
//  File:           AlertMetadataRules.cs
//  Created:        2026-05-26
//  Description:
//      Domain rules for metadata keys and values. Ensures normalization,
//      allowed characters, and additive-only evolution.
// ============================================================================
using System.Text.RegularExpressions;

namespace PlantSafety.Domain.Operational.Alerts.Rules;

public static class AlertMetadataRules
{
    private static readonly Regex _keyPattern = new("^[A-Za-z0-9_.-]+$", RegexOptions.Compiled);

    public static bool IsValidKey(string key)
        => !string.IsNullOrWhiteSpace(key) &&
           _keyPattern.IsMatch(key);

    public static string NormalizeValue(string value)
        => value.Trim();
}
