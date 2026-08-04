// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-04
// FILE DESCRIPTION: Singleton state container for the Sustainability.AdvancedMetrics dashboard
//   page. Holds the selected ledger's carbon footprint, energy/water intensity trends, waste
//   diversion rate, and sustainability target progress, plus loading/error state. Components
//   dispatch to this state via AdvancedMetricsApiClient.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Modules.Sustainability.AdvancedMetrics.Models;

namespace PlantSafety.UI.Modules.Sustainability.AdvancedMetrics.State;

/// <summary>Singleton state container for the Sustainability.AdvancedMetrics dashboard.</summary>
public sealed class AdvancedMetricsState
{
    private Guid? _selectedLedgerId;
    private CarbonFootprintByPeriodDto? _carbonFootprint;
    private IReadOnlyList<EnergyIntensityTrendPointDto> _energyTrend = [];
    private IReadOnlyList<WaterIntensityTrendPointDto> _waterTrend = [];
    private WasteDiversionRateDto? _wasteDiversionRate;
    private IReadOnlyList<SustainabilityTargetProgressDto> _targets = [];
    private bool _isLoading;
    private string? _loadError;
    private DateOnly _periodStart = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1));
    private DateOnly _periodEnd = DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>Gets the currently selected AdvancedMetricsLedger identifier, or null.</summary>
    public Guid? SelectedLedgerId => _selectedLedgerId;

    /// <summary>Gets the loaded carbon footprint rollup, or null when not loaded.</summary>
    public CarbonFootprintByPeriodDto? CarbonFootprint => _carbonFootprint;

    /// <summary>Gets the loaded energy-intensity trend points. Never null; empty when not loaded.</summary>
    public IReadOnlyList<EnergyIntensityTrendPointDto> EnergyTrend => _energyTrend;

    /// <summary>Gets the loaded water-intensity trend points. Never null; empty when not loaded.</summary>
    public IReadOnlyList<WaterIntensityTrendPointDto> WaterTrend => _waterTrend;

    /// <summary>Gets the loaded waste diversion rate rollup, or null when not loaded.</summary>
    public WasteDiversionRateDto? WasteDiversionRate => _wasteDiversionRate;

    /// <summary>Gets the loaded sustainability targets and progress. Never null; empty when not loaded.</summary>
    public IReadOnlyList<SustainabilityTargetProgressDto> Targets => _targets;

    /// <summary>Gets a value indicating whether the dashboard is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? LoadError => _loadError;

    /// <summary>Gets the inclusive start of the carbon footprint reporting period filter.</summary>
    public DateOnly PeriodStart => _periodStart;

    /// <summary>Gets the inclusive end of the carbon footprint reporting period filter.</summary>
    public DateOnly PeriodEnd => _periodEnd;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Sets the selected AdvancedMetricsLedger identifier.</summary>
    public void SelectLedger(Guid? ledgerId) { _selectedLedgerId = ledgerId; NotifyStateChanged(); }

    /// <summary>Sets the carbon footprint reporting period filter.</summary>
    public void SetPeriod(DateOnly start, DateOnly end) { _periodStart = start; _periodEnd = end; NotifyStateChanged(); }

    /// <summary>Marks the dashboard as loading.</summary>
    public void BeginLoad() { _isLoading = true; _loadError = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded carbon footprint rollup.</summary>
    public void SetCarbonFootprint(CarbonFootprintByPeriodDto? value) { _carbonFootprint = value; _isLoading = false; _loadError = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded energy/water intensity trends, waste diversion rate, and target progress.</summary>
    public void SetDashboardData(
        IReadOnlyList<EnergyIntensityTrendPointDto> energyTrend,
        IReadOnlyList<WaterIntensityTrendPointDto> waterTrend,
        WasteDiversionRateDto? wasteDiversionRate,
        IReadOnlyList<SustainabilityTargetProgressDto> targets)
    {
        _energyTrend = energyTrend;
        _waterTrend = waterTrend;
        _wasteDiversionRate = wasteDiversionRate;
        _targets = targets;
        _isLoading = false;
        _loadError = null;
        NotifyStateChanged();
    }

    /// <summary>Records a load failure with a sanitized message.</summary>
    public void SetLoadError(string sanitizedError) { _loadError = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
