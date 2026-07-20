// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-20
// FILE DESCRIPTION: Singleton state container for the Operations.Archival module. Holds
//   purge targets, purge history, retention settings, the client-computed data size
//   forecast, and trigger/loading/error state for the ArchivalDashboardPage.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Modules.OperationsArchival.Models;

namespace PlantSafety.UI.Modules.OperationsArchival.State;

/// <summary>Singleton state container for the Operations.Archival module.</summary>
public sealed class ArchivalState
{
    private IReadOnlyList<PurgeTargetDto> _targets = [];
    private IReadOnlyList<ArchivalJobDto> _history = [];
    private IReadOnlyList<TierPolicyDto> _retentionSettings = [];
    private IReadOnlyList<DataSizeForecastDto> _forecasts = [];
    private bool _isLoading;
    private bool _isPurging;
    private string? _error;
    private string? _purgeMessage;

    /// <summary>Gets static metadata for the 5 live purge targets.</summary>
    public IReadOnlyList<PurgeTargetDto> Targets => _targets;

    /// <summary>Gets the recent purge run history, newest first.</summary>
    public IReadOnlyList<ArchivalJobDto> History => _history;

    /// <summary>Gets the current retention windows (read-only).</summary>
    public IReadOnlyList<TierPolicyDto> RetentionSettings => _retentionSettings;

    /// <summary>Gets the client-computed data size forecasts (see DataSizeForecastDto for the approximation method).</summary>
    public IReadOnlyList<DataSizeForecastDto> Forecasts => _forecasts;

    /// <summary>Gets a value indicating whether the dashboard data is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a value indicating whether a purge trigger request is in flight.</summary>
    public bool IsPurging => _isPurging;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? Error => _error;

    /// <summary>Gets a status message describing the outcome of the last purge trigger action.</summary>
    public string? PurgeMessage => _purgeMessage;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the dashboard as loading.</summary>
    public void BeginLoad() { _isLoading = true; _error = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded dashboard data.</summary>
    public void SetData(
        IReadOnlyList<PurgeTargetDto> targets,
        IReadOnlyList<ArchivalJobDto> history,
        IReadOnlyList<TierPolicyDto> retentionSettings,
        IReadOnlyList<DataSizeForecastDto> forecasts)
    {
        _targets = targets;
        _history = history;
        _retentionSettings = retentionSettings;
        _forecasts = forecasts;
        _isLoading = false;
        _error = null;
        NotifyStateChanged();
    }

    /// <summary>Records a dashboard load failure with a sanitized message.</summary>
    public void SetError(string sanitizedError) { _error = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    /// <summary>Marks a purge trigger as in flight.</summary>
    public void BeginPurge() { _isPurging = true; _purgeMessage = null; NotifyStateChanged(); }

    /// <summary>Records the outcome of a purge trigger request.</summary>
    public void SetPurgeMessage(string message) { _purgeMessage = message; _isPurging = false; NotifyStateChanged(); }

    /// <summary>Records freshly loaded purge history (after a trigger, without a full dashboard reload).</summary>
    public void SetHistory(IReadOnlyList<ArchivalJobDto> history) { _history = history; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
