// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Shared validation rule constants and predicates used across all module validators.
//   Centralises values (max lengths, allowed enums) that are referenced by multiple validators.
// SchemaVersion: 1

namespace PlantSafety.Application.Validation.Rules;

/// <summary>
/// Shared validation constants and predicates for PlantSafety validators.
/// </summary>
public static class CommonValidationRules
{
    // --- String length limits ---

    /// <summary>Maximum length for short title fields.</summary>
    public const int MaxTitleLength = 200;

    /// <summary>Maximum length for standard description fields.</summary>
    public const int MaxDescriptionLength = 4000;

    /// <summary>Maximum length for location/area fields.</summary>
    public const int MaxLocationLength = 500;

    /// <summary>Maximum length for notes/reason fields.</summary>
    public const int MaxNotesLength = 1000;

    /// <summary>Maximum length for short code fields (permit number, reference, etc.).</summary>
    public const int MaxReferenceLength = 50;

    // --- Common severity values ---

    /// <summary>Allowed severity values used by Incidents, NearMisses, and CAPAs.</summary>
    public static readonly IReadOnlyList<string> ValidSeverities =
        ["Minor", "Moderate", "Serious", "Critical"];

    /// <summary>Allowed priority values used by CAPAs, Projects, and Alerts.</summary>
    public static readonly IReadOnlyList<string> ValidPriorities =
        ["Low", "Medium", "High", "Critical"];

    /// <summary>Allowed CAPA action types.</summary>
    public static readonly IReadOnlyList<string> ValidActionTypes = ["Corrective", "Preventive"];

    /// <summary>Allowed sort directions.</summary>
    public static readonly IReadOnlyList<string> ValidSortDirections = ["ASC", "DESC"];

    /// <summary>Allowed export format values.</summary>
    public static readonly IReadOnlyList<string> ValidExportFormats = ["csv", "json"];

    // --- Predicates ---

    /// <summary>Returns true when the date is in UTC (offset is zero).</summary>
    public static bool IsUtc(DateTimeOffset dt) => dt.Offset == TimeSpan.Zero;

    /// <summary>Returns true when the nullable date is null or in UTC.</summary>
    public static bool IsUtcOrNull(DateTimeOffset? dt) => dt is null || dt.Value.Offset == TimeSpan.Zero;

    /// <summary>Returns true when the string is a valid ISO 4217 currency code.</summary>
    public static bool IsValidCurrencyCode(string code) =>
        code is { Length: 3 } && code.All(char.IsUpper);
}
