// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Localization abstraction for the Application layer.
//   Provides locale-aware string resolution for user-facing messages and labels.
//   Locale-dependent normalization that changes domain meaning is prohibited (§14.1).
// SchemaVersion: 1
// Additive-only evolution: new members may be added; no members may be removed or renamed.

namespace PlantSafety.Application.Abstractions.Localization;

/// <summary>
/// Provides locale-aware string resolution for user-facing messages.
/// Infrastructure provides the resource-file backed implementation.
/// Application layer uses this only for user-facing output, never for domain logic.
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Resolves a localized string by key for the active locale.
    /// Returns the key itself if no translation is found (graceful degradation).
    /// </summary>
    /// <param name="key">The resource key for the string.</param>
    /// <returns>The localized string, or the key if not found.</returns>
    string GetString(string key);

    /// <summary>
    /// Resolves a localized string by key with named format arguments.
    /// </summary>
    /// <param name="key">The resource key for the string.</param>
    /// <param name="arguments">Named format arguments to interpolate.</param>
    /// <returns>The formatted localized string.</returns>
    string GetString(string key, params object[] arguments);

    /// <summary>Gets the active locale/culture code (e.g., <c>en-US</c>, <c>fr-CA</c>).</summary>
    string CurrentLocale { get; }
}
