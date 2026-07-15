// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Predictive Safety module.
//   Holds the loaded prediction window, the selected prediction for the detail
//   drilldown, and loading/error state. Components dispatch to this state via
//   PredictiveSafetyApiClient.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;

using PlantSafety.UI.Modules.PredictiveSafety.Models;

namespace PlantSafety.UI.Modules.PredictiveSafety.State;

/// <summary>Singleton state container for the Predictive Safety module.</summary>
public sealed class PredictiveSafetyState
{
    private IReadOnlyList<PredictiveSafetyItemDto> _predictions = [];
    private PredictiveSafetyItemDto? _selected;
    private bool _isLoading;
    private string? _loadError;
    private DateOnly _rangeStart = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-90));
    private DateOnly _rangeEnd = DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>Gets the currently loaded predictions. Never null; empty when not loaded.</summary>
    public IReadOnlyList<PredictiveSafetyItemDto> Predictions => _predictions;

    /// <summary>Gets the currently selected prediction for the detail view, or null.</summary>
    public PredictiveSafetyItemDto? Selected => _selected;

    /// <summary>Gets a value indicating whether the list is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? LoadError => _loadError;

    /// <summary>Gets the inclusive start of the query window.</summary>
    public DateOnly RangeStart => _rangeStart;

    /// <summary>Gets the inclusive end of the query window.</summary>
    public DateOnly RangeEnd => _rangeEnd;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Sets the query date range.</summary>
    public void SetRange(DateOnly start, DateOnly end) { _rangeStart = start; _rangeEnd = end; NotifyStateChanged(); }

    /// <summary>Marks the list as loading.</summary>
    public void BeginLoad() { _isLoading = true; _loadError = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded set of predictions.</summary>
    /// <param name="predictions">The prediction items returned by the API.</param>
    public void SetPredictions(IReadOnlyList<PredictiveSafetyItemDto> predictions)
    {
        _predictions = predictions;
        _isLoading = false;
        _loadError = null;
        NotifyStateChanged();
    }

    /// <summary>Records a load failure with a sanitized message.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetLoadError(string sanitizedError) { _loadError = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    /// <summary>Selects a prediction by identifier from the currently loaded set, if present.</summary>
    /// <param name="predictionId">The prediction identifier to select.</param>
    /// <returns>True if the prediction was found and selected.</returns>
    public bool SelectById(Guid predictionId)
    {
        _selected = _predictions.FirstOrDefault(p => p.PredictionId == predictionId);
        NotifyStateChanged();
        return _selected is not null;
    }

    /// <summary>Clears the selected prediction.</summary>
    public void ClearSelection() { _selected = null; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
