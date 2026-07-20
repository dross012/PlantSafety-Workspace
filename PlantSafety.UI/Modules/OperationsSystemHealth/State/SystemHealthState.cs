// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-20
// FILE DESCRIPTION: Singleton state container for the Operations.SystemHealth module.
//   Holds the unified rollup report plus loading/error state for
//   SystemHealthDashboardPage.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;

using PlantSafety.UI.Modules.OperationsSystemHealth.Models;

namespace PlantSafety.UI.Modules.OperationsSystemHealth.State;

/// <summary>Singleton state container for the Operations.SystemHealth module.</summary>
public sealed class SystemHealthState
{
    private SystemHealthReportDto? _report;
    private bool _isLoading;
    private string? _error;

    /// <summary>Gets the unified System Health rollup report, or null before the first load.</summary>
    public SystemHealthReportDto? Report => _report;

    /// <summary>Gets a value indicating whether the dashboard is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? Error => _error;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the dashboard as loading.</summary>
    public void BeginLoad() { _isLoading = true; _error = null; NotifyStateChanged(); }

    /// <summary>Applies the freshly loaded report.</summary>
    public void SetReport(SystemHealthReportDto report)
    {
        _report = report;
        _isLoading = false;
        _error = null;
        NotifyStateChanged();
    }

    /// <summary>Records a dashboard load failure with a sanitized message.</summary>
    public void SetError(string sanitizedError) { _error = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
