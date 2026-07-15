// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Trend Analysis page.
//   Holds loading/error/data state for the single Trend Analysis section
//   plus the shared date-range filter and the metric selector (Volume A1
//   §Page 2 MetricSelectorComponent). Components never call HttpClient
//   directly — they dispatch to this state via TrendAnalysisApiClient.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Modules.TrendAnalysis.Models;

namespace PlantSafety.UI.Modules.TrendAnalysis.State;

/// <summary>The tabs presented on the Trend Analysis page.</summary>
public enum TrendAnalysisTab
{
    /// <summary>Trend chart tab.</summary>
    TrendChart,

    /// <summary>Breakdown table tab.</summary>
    Breakdown
}

/// <summary>Singleton state container for the Trend Analysis page.</summary>
public sealed class TrendAnalysisState
{
    private TrendAnalysisTab _activeTab = TrendAnalysisTab.TrendChart;

    private DateOnly _filterStart = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-90));
    private DateOnly _filterEnd = DateOnly.FromDateTime(DateTime.UtcNow);

    private string? _selectedMetric;

    private IReadOnlyList<TrendAnalysisPointDto> _points = [];
    private bool _isLoading;
    private string? _error;

    /// <summary>Gets the currently active tab.</summary>
    public TrendAnalysisTab ActiveTab => _activeTab;

    /// <summary>Gets the inclusive filter start date.</summary>
    public DateOnly FilterStart => _filterStart;

    /// <summary>Gets the inclusive filter end date.</summary>
    public DateOnly FilterEnd => _filterEnd;

    /// <summary>Gets the currently selected metric filter, or null for "all metrics".</summary>
    public string? SelectedMetric => _selectedMetric;

    /// <summary>Gets the loaded trend data points.</summary>
    public IReadOnlyList<TrendAnalysisPointDto> Points => _points;

    /// <summary>Gets a value indicating whether data is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? Error => _error;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Switches the active tab.</summary>
    public void SetActiveTab(TrendAnalysisTab tab) { _activeTab = tab; NotifyStateChanged(); }

    /// <summary>Applies a new date-range filter. Callers MUST reload afterward.</summary>
    public void SetFilter(DateOnly start, DateOnly end)
    {
        _filterStart = start;
        _filterEnd = end;
        _points = [];
        NotifyStateChanged();
    }

    /// <summary>Applies the selected metric filter (client-side, applied to already-loaded points).</summary>
    public void SetSelectedMetric(string? metric) { _selectedMetric = metric; NotifyStateChanged(); }

    /// <summary>Marks the trend points as loading.</summary>
    public void BeginLoad() { _isLoading = true; _error = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded trend data points.</summary>
    public void SetPoints(IReadOnlyList<TrendAnalysisPointDto> items) { _points = items; _isLoading = false; _error = null; NotifyStateChanged(); }

    /// <summary>Records a load failure.</summary>
    public void SetError(string sanitizedError) { _error = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
