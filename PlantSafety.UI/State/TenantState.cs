// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-06-17
// FILE DESCRIPTION: Singleton state container for the active tenant descriptor.
//   Drives per-tenant module visibility, feature flags, and UI theming.
//   Populated after successful authentication by the AppShell component.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;

using PlantSafety.UI.Models.Auth;

namespace PlantSafety.UI.State;

/// <summary>
/// Singleton state container for the currently active tenant.
/// Injected via <c>@inject TenantState</c>; drives NavMenu module visibility.
/// </summary>
public sealed class TenantState
{
    private TenantDto? _tenant;

    /// <summary>Gets the active tenant descriptor, or null if not yet loaded.</summary>
    public TenantDto? Tenant => _tenant;

    /// <summary>Gets a value indicating whether tenant data has been loaded.</summary>
    public bool IsLoaded => _tenant is not null;

    /// <summary>Raised whenever the tenant state changes.</summary>
    public event Action? OnChange;

    /// <summary>
    /// Applies the tenant descriptor loaded from the API after authentication.
    /// </summary>
    /// <param name="tenant">The tenant descriptor returned by /auth/me.</param>
    public void SetTenant(TenantDto tenant)
    {
        _tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
        NotifyStateChanged();
    }

    /// <summary>Clears the tenant state on sign-out.</summary>
    public void ClearTenant()
    {
        _tenant = null;
        NotifyStateChanged();
    }

    /// <summary>
    /// Determines whether the given module is enabled for the active tenant.
    /// Returns false when tenant data has not yet been loaded.
    /// </summary>
    /// <param name="moduleCode">The module code (e.g., "alerts", "permits").</param>
    /// <returns>True if the module is enabled; otherwise false.</returns>
    public bool IsModuleEnabled(string moduleCode) =>
        _tenant?.IsModuleEnabled(moduleCode) ?? false;

    private void NotifyStateChanged() => OnChange?.Invoke();
}
