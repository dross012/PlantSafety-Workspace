// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-06-17
// FILE DESCRIPTION: Client-side tenant descriptor returned by the /auth/me
//   API endpoint. Used by TenantState to drive per-tenant UI customization
//   (branding, module visibility, feature flags).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Models.Auth;

/// <summary>
/// Immutable descriptor for the authenticated user's current tenant.
/// </summary>
public sealed class TenantDto
{
    /// <summary>Gets the unique tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the tenant's display name shown in the UI shell.</summary>
    public string TenantName { get; init; } = string.Empty;

    /// <summary>Gets the short slug used in API routing (URL-safe, lowercase).</summary>
    public string TenantSlug { get; init; } = string.Empty;

    /// <summary>Gets the list of module codes enabled for this tenant.</summary>
    public IReadOnlyList<string> EnabledModules { get; init; } = [];

    /// <summary>Gets the list of active feature flags for this tenant.</summary>
    public IReadOnlyList<string> FeatureFlags { get; init; } = [];

    /// <summary>Gets the tenant's primary brand color hex value for UI theming.</summary>
    public string BrandColor { get; init; } = "#1A3C6B";

    /// <summary>Gets the URL of the tenant's logo for display in the nav shell.</summary>
    public string LogoUrl { get; init; } = string.Empty;

    /// <summary>
    /// Determines whether a given module code is enabled for this tenant.
    /// </summary>
    /// <param name="moduleCode">The module code to check (e.g., "alerts", "permits").</param>
    /// <returns>True if the module is enabled; otherwise false.</returns>
    public bool IsModuleEnabled(string moduleCode) =>
        EnabledModules.Contains(moduleCode, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether a feature flag is active for this tenant.
    /// </summary>
    /// <param name="flag">The feature flag identifier.</param>
    /// <returns>True if the flag is active; otherwise false.</returns>
    public bool HasFeatureFlag(string flag) =>
        FeatureFlags.Contains(flag, StringComparer.OrdinalIgnoreCase);
}
