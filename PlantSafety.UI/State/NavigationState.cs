// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-06-17
// FILE DESCRIPTION: Singleton state container for UI navigation context.
//   Tracks the active module, breadcrumb trail, and page title for the
//   MainLayout shell. Updated by each page on initialization.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.State;

/// <summary>Represents a single breadcrumb segment in the navigation trail.</summary>
public sealed class BreadcrumbItem
{
    /// <summary>Gets the display label for this breadcrumb segment.</summary>
    public string Label { get; init; } = string.Empty;

    /// <summary>Gets the route href for this breadcrumb, or null for the current (non-linked) segment.</summary>
    public string? Href { get; init; }
}

/// <summary>
/// Singleton state container for shell-level navigation context.
/// Pages MUST update this state in OnInitialized to keep the TopBar accurate.
/// </summary>
public sealed class NavigationState
{
    private string _pageTitle = "PlantSafety";
    private string _activeModuleCode = string.Empty;
    private IReadOnlyList<BreadcrumbItem> _breadcrumbs = [];

    /// <summary>Gets the title of the currently active page.</summary>
    public string PageTitle => _pageTitle;

    /// <summary>Gets the module code of the currently active module (e.g., "alerts").</summary>
    public string ActiveModuleCode => _activeModuleCode;

    /// <summary>Gets the current breadcrumb trail.</summary>
    public IReadOnlyList<BreadcrumbItem> Breadcrumbs => _breadcrumbs;

    /// <summary>Raised when navigation context changes; MainLayout subscribes to re-render the TopBar.</summary>
    public event Action? OnChange;

    /// <summary>
    /// Sets the full navigation context for the active page.
    /// </summary>
    /// <param name="pageTitle">The page title shown in the top bar.</param>
    /// <param name="activeModuleCode">The module code of the active module.</param>
    /// <param name="breadcrumbs">The breadcrumb trail for the active page.</param>
    public void SetContext(
        string pageTitle,
        string activeModuleCode,
        IReadOnlyList<BreadcrumbItem> breadcrumbs)
    {
        _pageTitle = pageTitle ?? "PlantSafety";
        _activeModuleCode = activeModuleCode ?? string.Empty;
        _breadcrumbs = breadcrumbs ?? [];
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
