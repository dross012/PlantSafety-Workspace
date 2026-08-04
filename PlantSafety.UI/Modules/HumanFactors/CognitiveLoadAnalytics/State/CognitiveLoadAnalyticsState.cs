// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-04
// FILE DESCRIPTION: Singleton state container for the HumanFactors.CognitiveLoadAnalytics
//   dashboard page. Holds the selected worker's cognitive-load rollup, the selected shift's
//   workload-trend snapshots, and the elevated cognitive-risk prediction board, plus
//   loading/error state. Components dispatch to this state via CognitiveLoadAnalyticsApiClient.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Modules.HumanFactors.CognitiveLoadAnalytics.Models;

namespace PlantSafety.UI.Modules.HumanFactors.CognitiveLoadAnalytics.State;

/// <summary>Singleton state container for the HumanFactors.CognitiveLoadAnalytics dashboard.</summary>
public sealed class CognitiveLoadAnalyticsState
{
    private Guid? _selectedWorkerUserId;
    private CognitiveLoadByWorkerDto? _cognitiveLoadByWorker;
    private IReadOnlyList<WorkloadTrendByShiftDto> _workloadTrend = [];
    private IReadOnlyList<CognitiveRiskPredictionDto> _elevatedPredictions = [];
    private bool _isLoading;
    private string? _loadError;
    private DateOnly _periodStart = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7));
    private DateOnly _periodEnd = DateOnly.FromDateTime(DateTime.UtcNow);
    private string _selectedShiftType = "Day";

    /// <summary>Gets the currently selected worker identifier, or null.</summary>
    public Guid? SelectedWorkerUserId => _selectedWorkerUserId;

    /// <summary>Gets the loaded cognitive-load-by-worker rollup, or null when not loaded.</summary>
    public CognitiveLoadByWorkerDto? CognitiveLoadByWorker => _cognitiveLoadByWorker;

    /// <summary>Gets the loaded workload-trend snapshots. Never null; empty when not loaded.</summary>
    public IReadOnlyList<WorkloadTrendByShiftDto> WorkloadTrend => _workloadTrend;

    /// <summary>Gets the loaded elevated/critical cognitive-risk predictions. Never null; empty when not loaded.</summary>
    public IReadOnlyList<CognitiveRiskPredictionDto> ElevatedPredictions => _elevatedPredictions;

    /// <summary>Gets a value indicating whether the dashboard is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? LoadError => _loadError;

    /// <summary>Gets the inclusive start of the reporting period filter.</summary>
    public DateOnly PeriodStart => _periodStart;

    /// <summary>Gets the inclusive end of the reporting period filter.</summary>
    public DateOnly PeriodEnd => _periodEnd;

    /// <summary>Gets the currently selected shift type filter.</summary>
    public string SelectedShiftType => _selectedShiftType;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Sets the selected worker identifier.</summary>
    public void SelectWorker(Guid? workerUserId) { _selectedWorkerUserId = workerUserId; NotifyStateChanged(); }

    /// <summary>Sets the selected shift type filter.</summary>
    public void SelectShiftType(string shiftType) { _selectedShiftType = shiftType; NotifyStateChanged(); }

    /// <summary>Sets the reporting period filter.</summary>
    public void SetPeriod(DateOnly start, DateOnly end) { _periodStart = start; _periodEnd = end; NotifyStateChanged(); }

    /// <summary>Marks the dashboard as loading.</summary>
    public void BeginLoad() { _isLoading = true; _loadError = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded cognitive-load-by-worker rollup.</summary>
    public void SetCognitiveLoadByWorker(CognitiveLoadByWorkerDto? value) { _cognitiveLoadByWorker = value; _isLoading = false; _loadError = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded workload-trend snapshots and elevated risk predictions.</summary>
    public void SetDashboardData(IReadOnlyList<WorkloadTrendByShiftDto> workloadTrend, IReadOnlyList<CognitiveRiskPredictionDto> elevatedPredictions)
    {
        _workloadTrend = workloadTrend;
        _elevatedPredictions = elevatedPredictions;
        _isLoading = false;
        _loadError = null;
        NotifyStateChanged();
    }

    /// <summary>Records a load failure with a sanitized message.</summary>
    public void SetLoadError(string sanitizedError) { _loadError = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
