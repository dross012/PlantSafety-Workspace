// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Sustainability module.
//   Holds the loaded metric window, the selected metric for the detail
//   drilldown, and loading/error state. Components dispatch to this state via
//   SustainabilityApiClient.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;

using PlantSafety.UI.Modules.Sustainability.Models;

namespace PlantSafety.UI.Modules.Sustainability.State;

/// <summary>Singleton state container for the Sustainability module.</summary>
public sealed class SustainabilityState
{
    private IReadOnlyList<SustainabilityMetricDto> _metrics = [];
    private SustainabilityMetricDto? _selected;
    private bool _isLoading;
    private string? _loadError;
    private DateOnly _rangeStart = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-365));
    private DateOnly _rangeEnd = DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>Gets the currently loaded sustainability metrics. Never null; empty when not loaded.</summary>
    public IReadOnlyList<SustainabilityMetricDto> Metrics => _metrics;

    /// <summary>Gets the currently selected metric for the detail view, or null.</summary>
    public SustainabilityMetricDto? Selected => _selected;

    /// <summary>Gets a value indicating whether the metric list is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? LoadError => _loadError;

    /// <summary>Gets the inclusive start of the reporting window.</summary>
    public DateOnly RangeStart => _rangeStart;

    /// <summary>Gets the inclusive end of the reporting window.</summary>
    public DateOnly RangeEnd => _rangeEnd;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Sets the reporting window.</summary>
    public void SetRange(DateOnly start, DateOnly end) { _rangeStart = start; _rangeEnd = end; NotifyStateChanged(); }

    /// <summary>Marks the metric list as loading.</summary>
    public void BeginLoad() { _isLoading = true; _loadError = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded set of sustainability metrics.</summary>
    /// <param name="metrics">The metric items returned by the API.</param>
    public void SetMetrics(IReadOnlyList<SustainabilityMetricDto> metrics)
    {
        _metrics = metrics;
        _isLoading = false;
        _loadError = null;
        NotifyStateChanged();
    }

    /// <summary>Records a load failure with a sanitized message.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetLoadError(string sanitizedError) { _loadError = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    /// <summary>Selects a metric by identifier from the currently loaded set, if present.</summary>
    /// <param name="metricId">The metric identifier to select.</param>
    /// <returns>True if the metric was found and selected.</returns>
    public bool SelectById(Guid metricId)
    {
        _selected = _metrics.FirstOrDefault(m => m.MetricId == metricId);
        NotifyStateChanged();
        return _selected is not null;
    }

    /// <summary>Clears the selected metric.</summary>
    public void ClearSelection() { _selected = null; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
