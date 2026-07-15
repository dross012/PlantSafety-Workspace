// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Predictive Analytics
//   page. Holds independent loading/error/data state for each of the five
//   data-backed tabs (Model Output, Compliance Failure, Fatigue Risk,
//   Incident Prediction, Risk Forecasting) plus the shared date-range
//   filter. Components never call HttpClient directly — they dispatch to
//   this state via PredictiveAnalyticsApiClient.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Modules.PredictiveAnalytics.Models;

namespace PlantSafety.UI.Modules.PredictiveAnalytics.State;

/// <summary>The tabs presented on the Predictive Analytics page.</summary>
public enum PredictiveAnalyticsTab
{
    /// <summary>Top-level model output tab.</summary>
    ModelOutput,

    /// <summary>Compliance failure prediction tab.</summary>
    ComplianceFailure,

    /// <summary>Fatigue risk modeling tab.</summary>
    FatigueRisk,

    /// <summary>Incident prediction tab.</summary>
    IncidentPrediction,

    /// <summary>Risk forecasting tab.</summary>
    RiskForecasting
}

/// <summary>Singleton state container for the Predictive Analytics page.</summary>
public sealed class PredictiveAnalyticsState
{
    private PredictiveAnalyticsTab _activeTab = PredictiveAnalyticsTab.ModelOutput;

    private DateOnly _filterStart = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-90));
    private DateOnly _filterEnd = DateOnly.FromDateTime(DateTime.UtcNow);

    private IReadOnlyList<PredictiveModelOutputDto> _modelOutput = [];
    private IReadOnlyList<ComplianceFailureRiskDto> _complianceFailure = [];
    private IReadOnlyList<FatigueRiskAssessmentDto> _fatigueRisk = [];
    private IReadOnlyList<IncidentPredictionDto> _incidentPrediction = [];
    private IReadOnlyList<RiskForecastPointDto> _riskForecasting = [];

    private bool _isLoadingModelOutput;
    private bool _isLoadingComplianceFailure;
    private bool _isLoadingFatigueRisk;
    private bool _isLoadingIncidentPrediction;
    private bool _isLoadingRiskForecasting;

    private string? _modelOutputError;
    private string? _complianceFailureError;
    private string? _fatigueRiskError;
    private string? _incidentPredictionError;
    private string? _riskForecastingError;

    /// <summary>Gets the currently active tab.</summary>
    public PredictiveAnalyticsTab ActiveTab => _activeTab;

    /// <summary>Gets the inclusive filter start date.</summary>
    public DateOnly FilterStart => _filterStart;

    /// <summary>Gets the inclusive filter end date.</summary>
    public DateOnly FilterEnd => _filterEnd;

    /// <summary>Gets the loaded top-level model output.</summary>
    public IReadOnlyList<PredictiveModelOutputDto> ModelOutput => _modelOutput;

    /// <summary>Gets the loaded compliance failure predictions.</summary>
    public IReadOnlyList<ComplianceFailureRiskDto> ComplianceFailure => _complianceFailure;

    /// <summary>Gets the loaded fatigue risk assessments.</summary>
    public IReadOnlyList<FatigueRiskAssessmentDto> FatigueRisk => _fatigueRisk;

    /// <summary>Gets the loaded incident predictions.</summary>
    public IReadOnlyList<IncidentPredictionDto> IncidentPrediction => _incidentPrediction;

    /// <summary>Gets the loaded risk forecast points.</summary>
    public IReadOnlyList<RiskForecastPointDto> RiskForecasting => _riskForecasting;

    /// <summary>Gets a value indicating whether the Model Output tab is loading.</summary>
    public bool IsLoadingModelOutput => _isLoadingModelOutput;

    /// <summary>Gets a value indicating whether the Compliance Failure tab is loading.</summary>
    public bool IsLoadingComplianceFailure => _isLoadingComplianceFailure;

    /// <summary>Gets a value indicating whether the Fatigue Risk tab is loading.</summary>
    public bool IsLoadingFatigueRisk => _isLoadingFatigueRisk;

    /// <summary>Gets a value indicating whether the Incident Prediction tab is loading.</summary>
    public bool IsLoadingIncidentPrediction => _isLoadingIncidentPrediction;

    /// <summary>Gets a value indicating whether the Risk Forecasting tab is loading.</summary>
    public bool IsLoadingRiskForecasting => _isLoadingRiskForecasting;

    /// <summary>Gets a sanitized Model Output load error, or null.</summary>
    public string? ModelOutputError => _modelOutputError;

    /// <summary>Gets a sanitized Compliance Failure load error, or null.</summary>
    public string? ComplianceFailureError => _complianceFailureError;

    /// <summary>Gets a sanitized Fatigue Risk load error, or null.</summary>
    public string? FatigueRiskError => _fatigueRiskError;

    /// <summary>Gets a sanitized Incident Prediction load error, or null.</summary>
    public string? IncidentPredictionError => _incidentPredictionError;

    /// <summary>Gets a sanitized Risk Forecasting load error, or null.</summary>
    public string? RiskForecastingError => _riskForecastingError;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Switches the active tab.</summary>
    public void SetActiveTab(PredictiveAnalyticsTab tab) { _activeTab = tab; NotifyStateChanged(); }

    /// <summary>Applies a new date-range filter. Callers MUST reload the active tab afterward.</summary>
    public void SetFilter(DateOnly start, DateOnly end)
    {
        _filterStart = start;
        _filterEnd = end;
        _modelOutput = [];
        _complianceFailure = [];
        _fatigueRisk = [];
        _incidentPrediction = [];
        _riskForecasting = [];
        NotifyStateChanged();
    }

    /// <summary>Marks the Model Output tab as loading.</summary>
    public void BeginModelOutputLoad() { _isLoadingModelOutput = true; _modelOutputError = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded model output.</summary>
    public void SetModelOutput(IReadOnlyList<PredictiveModelOutputDto> items) { _modelOutput = items; _isLoadingModelOutput = false; _modelOutputError = null; NotifyStateChanged(); }

    /// <summary>Records a Model Output load failure.</summary>
    public void SetModelOutputError(string sanitizedError) { _modelOutputError = sanitizedError; _isLoadingModelOutput = false; NotifyStateChanged(); }

    /// <summary>Marks the Compliance Failure tab as loading.</summary>
    public void BeginComplianceFailureLoad() { _isLoadingComplianceFailure = true; _complianceFailureError = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded compliance failure predictions.</summary>
    public void SetComplianceFailure(IReadOnlyList<ComplianceFailureRiskDto> items) { _complianceFailure = items; _isLoadingComplianceFailure = false; _complianceFailureError = null; NotifyStateChanged(); }

    /// <summary>Records a Compliance Failure load failure.</summary>
    public void SetComplianceFailureError(string sanitizedError) { _complianceFailureError = sanitizedError; _isLoadingComplianceFailure = false; NotifyStateChanged(); }

    /// <summary>Marks the Fatigue Risk tab as loading.</summary>
    public void BeginFatigueRiskLoad() { _isLoadingFatigueRisk = true; _fatigueRiskError = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded fatigue risk assessments.</summary>
    public void SetFatigueRisk(IReadOnlyList<FatigueRiskAssessmentDto> items) { _fatigueRisk = items; _isLoadingFatigueRisk = false; _fatigueRiskError = null; NotifyStateChanged(); }

    /// <summary>Records a Fatigue Risk load failure.</summary>
    public void SetFatigueRiskError(string sanitizedError) { _fatigueRiskError = sanitizedError; _isLoadingFatigueRisk = false; NotifyStateChanged(); }

    /// <summary>Marks the Incident Prediction tab as loading.</summary>
    public void BeginIncidentPredictionLoad() { _isLoadingIncidentPrediction = true; _incidentPredictionError = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded incident predictions.</summary>
    public void SetIncidentPrediction(IReadOnlyList<IncidentPredictionDto> items) { _incidentPrediction = items; _isLoadingIncidentPrediction = false; _incidentPredictionError = null; NotifyStateChanged(); }

    /// <summary>Records an Incident Prediction load failure.</summary>
    public void SetIncidentPredictionError(string sanitizedError) { _incidentPredictionError = sanitizedError; _isLoadingIncidentPrediction = false; NotifyStateChanged(); }

    /// <summary>Marks the Risk Forecasting tab as loading.</summary>
    public void BeginRiskForecastingLoad() { _isLoadingRiskForecasting = true; _riskForecastingError = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded risk forecast points.</summary>
    public void SetRiskForecasting(IReadOnlyList<RiskForecastPointDto> items) { _riskForecasting = items; _isLoadingRiskForecasting = false; _riskForecastingError = null; NotifyStateChanged(); }

    /// <summary>Records a Risk Forecasting load failure.</summary>
    public void SetRiskForecastingError(string sanitizedError) { _riskForecastingError = sanitizedError; _isLoadingRiskForecasting = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
