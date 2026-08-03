// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-03
// FILE DESCRIPTION: Singleton state container for the Admin/Executive Global Safety Dashboard
//   (Phase 8 §5 Multi-Site Safety Intelligence Network) -- cross-site KPI summary, anomaly/risk
//   detail, and cross-site coordinated-action audit trail.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Modules.Admin.Models;

namespace PlantSafety.UI.Modules.Admin.State;

/// <summary>Singleton state container for the Admin/Executive Global Safety Dashboard.</summary>
public sealed class GlobalSafetyDashboardState
{
    private GlobalSafetyInsightsQueryResultDto? _summary;
    private GlobalSafetyInsightsDetailDto? _detail;
    private IReadOnlyList<CrossSiteActionLogEntryDto> _crossSiteActions = [];
    private bool _isLoading;
    private string? _error;

    /// <summary>Gets the cross-site KPI summary plus history, or null when not loaded.</summary>
    public GlobalSafetyInsightsQueryResultDto? Summary => _summary;

    /// <summary>Gets the full detail (risk scores/anomalies/recommendations) of the latest snapshot, or null when not loaded.</summary>
    public GlobalSafetyInsightsDetailDto? Detail => _detail;

    /// <summary>Gets every loaded cross-site coordinated-action audit entry, most recent first.</summary>
    public IReadOnlyList<CrossSiteActionLogEntryDto> CrossSiteActions => _crossSiteActions;

    /// <summary>Gets a value indicating whether the dashboard is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? Error => _error;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the dashboard as loading.</summary>
    public void BeginLoad() { _isLoading = true; _error = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded dashboard data.</summary>
    public void SetData(
        GlobalSafetyInsightsQueryResultDto? summary,
        GlobalSafetyInsightsDetailDto? detail,
        IReadOnlyList<CrossSiteActionLogEntryDto> crossSiteActions)
    {
        _summary = summary;
        _detail = detail;
        _crossSiteActions = crossSiteActions;
        _isLoading = false;
        _error = null;
        NotifyStateChanged();
    }

    /// <summary>Records a dashboard load failure.</summary>
    public void SetError(string sanitizedError) { _error = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
