// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state containers for the Predictions List,
//   Sensor Data Explorer, Recommendation Detail, Equipment Predictive View,
//   and Predictive Reports pages (Volume P1 §3/§5/§6/§2/§7).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Models.Common;
using PlantSafety.UI.Modules.PredictiveMaintenance.Models;

namespace PlantSafety.UI.Modules.PredictiveMaintenance.State;

/// <summary>Singleton state container for the Predictions List page.</summary>
public sealed class PredictionsListState
{
    private IReadOnlyList<PredictionRowDto> _predictions = [];
    private bool _isLoading;
    private string? _errorMessage;
    private int _currentPage = 1;
    private int _totalCount;
    private int _totalPages;

    /// <summary>Gets the current page of predictions.</summary>
    public IReadOnlyList<PredictionRowDto> Predictions => _predictions;

    /// <summary>Gets a value indicating whether the page is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? ErrorMessage => _errorMessage;

    /// <summary>Gets the current page number.</summary>
    public int CurrentPage => _currentPage;

    /// <summary>Gets the total matching count.</summary>
    public int TotalCount => _totalCount;

    /// <summary>Gets the total page count.</summary>
    public int TotalPages => _totalPages;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the page as loading.</summary>
    public void BeginLoad() { _isLoading = true; _errorMessage = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded page of predictions.</summary>
    public void SetPredictions(PaginatedResponseDto<PredictionRowDto> page)
    {
        _predictions = page.Items;
        _currentPage = page.PageNumber;
        _totalCount = page.TotalCount;
        _totalPages = page.TotalPages;
        _isLoading = false;
        _errorMessage = null;
        NotifyStateChanged();
    }

    /// <summary>Records a load failure with a sanitized message.</summary>
    public void SetError(string sanitizedError) { _errorMessage = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}

/// <summary>Singleton state container for the Sensor Data Explorer page.</summary>
public sealed class SensorExplorerState
{
    private IReadOnlyList<SensorDataPointDto> _sensorData = [];
    private IReadOnlyList<SensorAnomalyDto> _anomalies = [];
    private bool _isLoading;
    private string? _errorMessage;

    /// <summary>Gets the multi-channel sensor data currently loaded.</summary>
    public IReadOnlyList<SensorDataPointDto> SensorData => _sensorData;

    /// <summary>Gets the detected anomalies currently loaded.</summary>
    public IReadOnlyList<SensorAnomalyDto> Anomalies => _anomalies;

    /// <summary>Gets a value indicating whether the explorer is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? ErrorMessage => _errorMessage;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the explorer as loading.</summary>
    public void BeginLoad() { _isLoading = true; _errorMessage = null; NotifyStateChanged(); }

    /// <summary>Applies loaded sensor data and anomalies.</summary>
    public void SetData(IReadOnlyList<SensorDataPointDto> sensorData, IReadOnlyList<SensorAnomalyDto> anomalies)
    {
        _sensorData = sensorData;
        _anomalies = anomalies;
        _isLoading = false;
        _errorMessage = null;
        NotifyStateChanged();
    }

    /// <summary>Records a load failure with a sanitized message.</summary>
    public void SetError(string sanitizedError) { _errorMessage = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}

/// <summary>Singleton state container for the Recommendation Detail page.</summary>
public sealed class RecommendationDetailState
{
    private RecommendationDto? _recommendation;
    private bool _isLoading;
    private string? _errorMessage;

    /// <summary>Gets the loaded recommendation, or null when not yet loaded.</summary>
    public RecommendationDto? Recommendation => _recommendation;

    /// <summary>Gets a value indicating whether the page is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? ErrorMessage => _errorMessage;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the page as loading.</summary>
    public void BeginLoad() { _isLoading = true; _errorMessage = null; NotifyStateChanged(); }

    /// <summary>Applies the loaded recommendation.</summary>
    public void SetRecommendation(RecommendationDto recommendation) { _recommendation = recommendation; _isLoading = false; _errorMessage = null; NotifyStateChanged(); }

    /// <summary>Records a load failure with a sanitized message.</summary>
    public void SetError(string sanitizedError) { _errorMessage = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}

/// <summary>Singleton state container for the Equipment Detail (Predictive View) page.</summary>
public sealed class EquipmentPredictiveViewState
{
    private EquipmentPredictiveViewDto? _equipment;
    private bool _isLoading;
    private string? _errorMessage;

    /// <summary>Gets the loaded equipment predictive view, or null when not yet loaded.</summary>
    public EquipmentPredictiveViewDto? Equipment => _equipment;

    /// <summary>Gets a value indicating whether the page is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? ErrorMessage => _errorMessage;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the page as loading.</summary>
    public void BeginLoad() { _isLoading = true; _errorMessage = null; NotifyStateChanged(); }

    /// <summary>Applies the loaded equipment predictive view.</summary>
    public void SetEquipment(EquipmentPredictiveViewDto equipment) { _equipment = equipment; _isLoading = false; _errorMessage = null; NotifyStateChanged(); }

    /// <summary>Records a load failure with a sanitized message.</summary>
    public void SetError(string sanitizedError) { _errorMessage = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}

/// <summary>Singleton state container for the Predictive Reports page.</summary>
public sealed class PredictiveReportsState
{
    private IReadOnlyList<PredictiveReportDto> _reports = [];
    private bool _isLoading;
    private string? _errorMessage;

    /// <summary>Gets the reports currently loaded.</summary>
    public IReadOnlyList<PredictiveReportDto> Reports => _reports;

    /// <summary>Gets a value indicating whether the page is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? ErrorMessage => _errorMessage;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the page as loading.</summary>
    public void BeginLoad() { _isLoading = true; _errorMessage = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded page of reports.</summary>
    public void SetReports(PaginatedResponseDto<PredictiveReportDto> page)
    {
        _reports = page.Items;
        _isLoading = false;
        _errorMessage = null;
        NotifyStateChanged();
    }

    /// <summary>Records a load failure with a sanitized message.</summary>
    public void SetError(string sanitizedError) { _errorMessage = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
