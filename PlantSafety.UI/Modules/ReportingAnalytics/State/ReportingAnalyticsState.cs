// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Reporting Analytics
//   page. Holds loading/error/data state for the single Reporting section
//   plus the shared date-range filter. Components never call HttpClient
//   directly — they dispatch to this state via ReportingAnalyticsApiClient.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Modules.ReportingAnalytics.Models;

namespace PlantSafety.UI.Modules.ReportingAnalytics.State;

/// <summary>The tabs presented on the Reporting Analytics page.</summary>
public enum ReportingAnalyticsTab
{
    /// <summary>Overview summary tab.</summary>
    Overview,

    /// <summary>Full reports table tab.</summary>
    Reports,

    /// <summary>By-category breakdown tab.</summary>
    ByCategory
}

/// <summary>Singleton state container for the Reporting Analytics page.</summary>
public sealed class ReportingAnalyticsState
{
    private ReportingAnalyticsTab _activeTab = ReportingAnalyticsTab.Overview;

    private DateOnly _filterStart = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-90));
    private DateOnly _filterEnd = DateOnly.FromDateTime(DateTime.UtcNow);

    private IReadOnlyList<ReportEntryDto> _reports = [];
    private bool _isLoading;
    private string? _error;

    /// <summary>Gets the currently active tab.</summary>
    public ReportingAnalyticsTab ActiveTab => _activeTab;

    /// <summary>Gets the inclusive filter start date.</summary>
    public DateOnly FilterStart => _filterStart;

    /// <summary>Gets the inclusive filter end date.</summary>
    public DateOnly FilterEnd => _filterEnd;

    /// <summary>Gets the loaded report entries.</summary>
    public IReadOnlyList<ReportEntryDto> Reports => _reports;

    /// <summary>Gets a value indicating whether data is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? Error => _error;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Switches the active tab.</summary>
    public void SetActiveTab(ReportingAnalyticsTab tab) { _activeTab = tab; NotifyStateChanged(); }

    /// <summary>Applies a new date-range filter. Callers MUST reload afterward.</summary>
    public void SetFilter(DateOnly start, DateOnly end)
    {
        _filterStart = start;
        _filterEnd = end;
        _reports = [];
        NotifyStateChanged();
    }

    /// <summary>Marks the reports as loading.</summary>
    public void BeginLoad() { _isLoading = true; _error = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded report entries.</summary>
    public void SetReports(IReadOnlyList<ReportEntryDto> items) { _reports = items; _isLoading = false; _error = null; NotifyStateChanged(); }

    /// <summary>Records a load failure.</summary>
    public void SetError(string sanitizedError) { _error = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
