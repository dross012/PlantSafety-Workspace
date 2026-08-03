// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-03
// FILE DESCRIPTION: Singleton state container for the Admin Portal's per-tenant Feature Flags
//   page. Pairs each global feature flag with its resolved outcome for a specific tenant
//   (Phase 7 §7.1 4-level precedence: user > tenant > region > global), so the page can show
//   both the raw global setting and the precedence-resolved outcome side by side.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Modules.OperationsConfiguration.Models;

namespace PlantSafety.UI.Modules.Admin.State;

/// <summary>Pairs a global feature flag with its precedence-resolved outcome for one tenant.</summary>
public sealed class TenantFeatureFlagRowDto
{
    /// <summary>Gets the global flag row this resolution was evaluated against.</summary>
    public required FeatureFlagDto Global { get; init; }

    /// <summary>Gets the resolved outcome for the tenant, or null if resolution failed for this flag.</summary>
    public FeatureFlagResolutionResultDto? Resolved { get; init; }
}

/// <summary>Singleton state container for the Admin Portal's per-tenant Feature Flags page.</summary>
public sealed class TenantFeatureFlagsState
{
    private IReadOnlyList<TenantFeatureFlagRowDto> _rows = [];
    private bool _isLoading;
    private bool _isSubmittingOverride;
    private string? _error;
    private string? _overrideMessage;

    /// <summary>Gets every loaded flag paired with its resolution outcome for the current tenant.</summary>
    public IReadOnlyList<TenantFeatureFlagRowDto> Rows => _rows;

    /// <summary>Gets a value indicating whether the flag list is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a value indicating whether a tenant-override submission is in flight.</summary>
    public bool IsSubmittingOverride => _isSubmittingOverride;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? Error => _error;

    /// <summary>Gets the most recent override submission outcome message.</summary>
    public string? OverrideMessage => _overrideMessage;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the flag list as loading.</summary>
    public void BeginLoad() { _isLoading = true; _error = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded, resolved flag rows.</summary>
    public void SetRows(IReadOnlyList<TenantFeatureFlagRowDto> rows) { _rows = rows; _isLoading = false; _error = null; NotifyStateChanged(); }

    /// <summary>Records a load failure.</summary>
    public void SetError(string sanitizedError) { _error = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    /// <summary>Marks a tenant-override submission as in flight.</summary>
    public void BeginOverrideSubmit() { _isSubmittingOverride = true; _overrideMessage = null; NotifyStateChanged(); }

    /// <summary>Records the outcome of a completed tenant-override submission.</summary>
    public void CompleteOverrideSubmit(string message) { _isSubmittingOverride = false; _overrideMessage = message; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
