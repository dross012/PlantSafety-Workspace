// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Prediction Detail
//   page (Volume P1 §2). Holds summary, model explanation, forecast,
//   recommendations, and the active tab (Forecast/Sensors/Factors/
//   Recommendations/Metadata).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Modules.PredictiveMaintenance.Models;

namespace PlantSafety.UI.Modules.PredictiveMaintenance.State;

/// <summary>Canonical Prediction Detail tab identifiers, in display order.</summary>
public enum PredictionDetailTab
{
    /// <summary>Forecast tab (default).</summary>
    Forecast = 0,

    /// <summary>Sensors tab.</summary>
    Sensors = 1,

    /// <summary>Factors tab.</summary>
    Factors = 2,

    /// <summary>Recommendations tab.</summary>
    Recommendations = 3,

    /// <summary>Metadata tab.</summary>
    Metadata = 4,
}

/// <summary>Singleton state container for the Prediction Detail page.</summary>
public sealed class PredictionDetailState
{
    private PredictionSummaryDto? _summary;
    private ModelExplanationDto? _explanation;
    private IReadOnlyList<ForecastPointDto> _forecast = [];
    private IReadOnlyList<SensorDataPointDto> _sensorData = [];
    private IReadOnlyList<RecommendationDto> _recommendations = [];
    private PredictionDetailTab _activeTab = PredictionDetailTab.Forecast;
    private bool _isLoading;
    private bool _isLoadingTab;
    private string? _errorMessage;

    /// <summary>Gets the loaded prediction summary, or null when not yet loaded.</summary>
    public PredictionSummaryDto? Summary => _summary;

    /// <summary>Gets the model explanation, or null when not yet loaded.</summary>
    public ModelExplanationDto? Explanation => _explanation;

    /// <summary>Gets the risk forecast curve.</summary>
    public IReadOnlyList<ForecastPointDto> Forecast => _forecast;

    /// <summary>Gets the sensor data points for this prediction's equipment.</summary>
    public IReadOnlyList<SensorDataPointDto> SensorData => _sensorData;

    /// <summary>Gets the recommended actions.</summary>
    public IReadOnlyList<RecommendationDto> Recommendations => _recommendations;

    /// <summary>Gets the currently active tab.</summary>
    public PredictionDetailTab ActiveTab => _activeTab;

    /// <summary>Gets a value indicating whether the header is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a value indicating whether the active tab is loading.</summary>
    public bool IsLoadingTab => _isLoadingTab;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? ErrorMessage => _errorMessage;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the header as loading.</summary>
    public void BeginLoad() { _isLoading = true; _errorMessage = null; NotifyStateChanged(); }

    /// <summary>Applies the loaded prediction summary and model explanation.</summary>
    public void SetSummary(PredictionSummaryDto summary, ModelExplanationDto explanation)
    {
        _summary = summary;
        _explanation = explanation;
        _isLoading = false;
        _errorMessage = null;
        NotifyStateChanged();
    }

    /// <summary>Marks the active tab as loading.</summary>
    public void BeginTabLoad() { _isLoadingTab = true; NotifyStateChanged(); }

    /// <summary>Applies loaded forecast points, reordered chronologically.</summary>
    public void SetForecast(IReadOnlyList<ForecastPointDto> forecast) { _forecast = [.. forecast.OrderBy(p => p.TimestampUtc)]; _isLoadingTab = false; NotifyStateChanged(); }

    /// <summary>Applies loaded sensor data.</summary>
    public void SetSensorData(IReadOnlyList<SensorDataPointDto> sensorData) { _sensorData = sensorData; _isLoadingTab = false; NotifyStateChanged(); }

    /// <summary>Applies loaded recommendations.</summary>
    public void SetRecommendations(IReadOnlyList<RecommendationDto> recommendations) { _recommendations = recommendations; _isLoadingTab = false; NotifyStateChanged(); }

    /// <summary>Switches the active tab.</summary>
    public void SetActiveTab(PredictionDetailTab tab) { _activeTab = tab; NotifyStateChanged(); }

    /// <summary>Records a load failure with a sanitized message.</summary>
    public void SetError(string sanitizedError) { _errorMessage = sanitizedError; _isLoading = false; _isLoadingTab = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
