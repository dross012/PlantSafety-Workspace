// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-20
// FILE DESCRIPTION: Singleton state container for the Operations.Configuration module.
//   Holds the loaded feature flags and per-flag toggle/loading/error state for the
//   ConfigurationDashboardPage.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;

using PlantSafety.UI.Modules.OperationsConfiguration.Models;

namespace PlantSafety.UI.Modules.OperationsConfiguration.State;

/// <summary>Singleton state container for the Operations.Configuration module.</summary>
public sealed class ConfigurationState
{
    private IReadOnlyList<FeatureFlagDto> _flags = [];
    private bool _isLoading;
    private string? _error;
    private string? _toggleMessage;
    private string? _togglingFlagKey;

    /// <summary>Gets every loaded feature flag across all known databases.</summary>
    public IReadOnlyList<FeatureFlagDto> Flags => _flags;

    /// <summary>Gets a value indicating whether the dashboard data is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? Error => _error;

    /// <summary>Gets a status message describing the outcome of the last toggle action.</summary>
    public string? ToggleMessage => _toggleMessage;

    /// <summary>Gets the "DatabaseKey|FlagName" composite key of the flag currently being toggled, or null.</summary>
    public string? TogglingFlagKey => _togglingFlagKey;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Builds the composite key used to correlate a row with an in-flight toggle.</summary>
    public static string FlagKey(string databaseKey, string flagName) => $"{databaseKey}|{flagName}";

    /// <summary>Marks the dashboard as loading.</summary>
    public void BeginLoad() { _isLoading = true; _error = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded feature flags.</summary>
    public void SetData(IReadOnlyList<FeatureFlagDto> flags)
    {
        _flags = flags.OrderBy(f => f.DatabaseKey).ThenBy(f => f.FlagName).ToList();
        _isLoading = false;
        _error = null;
        NotifyStateChanged();
    }

    /// <summary>Records a dashboard load failure with a sanitized message.</summary>
    public void SetError(string sanitizedError) { _error = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    /// <summary>Marks a specific flag as in the middle of being toggled.</summary>
    public void BeginToggle(string databaseKey, string flagName)
    {
        _togglingFlagKey = FlagKey(databaseKey, flagName);
        _toggleMessage = null;
        NotifyStateChanged();
    }

    /// <summary>Applies the result of a successful toggle in-place.</summary>
    public void ApplyToggleResult(FeatureFlagDto updated)
    {
        _flags = _flags
            .Select(f => f.DatabaseKey == updated.DatabaseKey && f.FlagName == updated.FlagName ? updated : f)
            .ToList();
        _togglingFlagKey = null;
        _toggleMessage = $"{updated.FlagName} ({updated.DatabaseKey}) is now {(updated.IsEnabled ? "ENABLED" : "DISABLED")}.";
        NotifyStateChanged();
    }

    /// <summary>Records a toggle failure with a sanitized message.</summary>
    public void SetToggleError(string sanitizedError) { _toggleMessage = sanitizedError; _togglingFlagKey = null; NotifyStateChanged(); }

    // ── Phase 7 §7.1/§7.2 Tenant/User/Region overrides + release channel + rollback ──

    private string? _overridesMessage;
    private bool _isSubmittingOverride;

    /// <summary>Gets the most recent override/release-channel/rollback action outcome message.</summary>
    public string? OverridesMessage => _overridesMessage;

    /// <summary>Gets a value indicating whether an override/release-channel/rollback action is in flight.</summary>
    public bool IsSubmittingOverride => _isSubmittingOverride;

    /// <summary>Marks an override/release-channel/rollback action as in flight.</summary>
    public void BeginOverrideSubmit() { _isSubmittingOverride = true; _overridesMessage = null; NotifyStateChanged(); }

    /// <summary>Records the outcome of a completed override/release-channel/rollback action.</summary>
    public void SetOverrideResult(string message) { _isSubmittingOverride = false; _overridesMessage = message; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
