// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Predictive Dashboard
//   page (Volume P1 §1). Holds Risk Summary, Equipment Health, and Active
//   Predictions.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Models.Common;
using PlantSafety.UI.Modules.PredictiveMaintenance.Models;

namespace PlantSafety.UI.Modules.PredictiveMaintenance.State;

/// <summary>Singleton state container for the Predictive Dashboard.</summary>
public sealed class PredictiveDashboardState
{
    private RiskSummaryDto? _riskSummary;
    private EquipmentHealthDto? _equipmentHealth;
    private IReadOnlyList<PredictionRowDto> _activePredictions = [];
    private bool _isLoading;
    private string? _errorMessage;
    private int _currentPage = 1;
    private int _totalCount;
    private int _totalPages;

    /// <summary>Gets the Risk Summary card data, or null when not yet loaded.</summary>
    public RiskSummaryDto? RiskSummary => _riskSummary;

    /// <summary>Gets the Equipment Health card data, or null when not yet loaded.</summary>
    public EquipmentHealthDto? EquipmentHealth => _equipmentHealth;

    /// <summary>Gets the current page of active predictions.</summary>
    public IReadOnlyList<PredictionRowDto> ActivePredictions => _activePredictions;

    /// <summary>Gets a value indicating whether the dashboard is loading.</summary>
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

    /// <summary>Marks the dashboard as loading.</summary>
    public void BeginLoad() { _isLoading = true; _errorMessage = null; NotifyStateChanged(); }

    /// <summary>Applies the loaded summary cards.</summary>
    public void SetSummaries(RiskSummaryDto riskSummary, EquipmentHealthDto equipmentHealth)
    {
        _riskSummary = riskSummary;
        _equipmentHealth = equipmentHealth;
        NotifyStateChanged();
    }

    /// <summary>Applies a freshly loaded page of active predictions, updating only changed rows.</summary>
    public void SetActivePredictions(PaginatedResponseDto<PredictionRowDto> page)
    {
        _activePredictions = page.Items;
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
