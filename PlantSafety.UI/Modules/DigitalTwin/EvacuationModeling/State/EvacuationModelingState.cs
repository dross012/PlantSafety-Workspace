// ============================================================
// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Singleton state container for the DigitalTwin.EvacuationModeling module.
//   Holds active route obstructions and bottleneck history for the Live Evacuation Readiness
//   Board dashboard, plus loading/error state. Components dispatch to this state via
//   EvacuationModelingApiClient.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Modules.DigitalTwin.EvacuationModeling.Models;

namespace PlantSafety.UI.Modules.DigitalTwin.EvacuationModeling.State;

/// <summary>Singleton state container for the DigitalTwin.EvacuationModeling module.</summary>
public sealed class EvacuationModelingState
{
    private IReadOnlyList<RouteObstructionDto> _activeObstructions = [];
    private IReadOnlyList<EvacuationBottleneckDto> _bottleneckHistory = [];
    private bool _isLoading;
    private string? _loadError;

    private IReadOnlyList<EvacuationRouteDto> _comparisonPlannedRoutes = [];
    private EgressSimulationDto? _comparisonLatestSimulation;
    private bool _isLoadingComparison;
    private string? _comparisonError;
    private Guid? _comparisonZoneId;

    /// <summary>Gets the currently loaded active (uncleared) route obstructions. Never null; empty when not loaded.</summary>
    public IReadOnlyList<RouteObstructionDto> ActiveObstructions => _activeObstructions;

    /// <summary>Gets the currently loaded bottleneck history. Never null; empty when not loaded.</summary>
    public IReadOnlyList<EvacuationBottleneckDto> BottleneckHistory => _bottleneckHistory;

    /// <summary>Gets a value indicating whether the readiness board is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? LoadError => _loadError;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the readiness board as loading.</summary>
    public void BeginLoad() { _isLoading = true; _loadError = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded obstruction and bottleneck data.</summary>
    /// <param name="activeObstructions">Active obstructions returned by the API.</param>
    /// <param name="bottleneckHistory">Bottleneck history returned by the API.</param>
    public void SetReadinessData(IReadOnlyList<RouteObstructionDto> activeObstructions, IReadOnlyList<EvacuationBottleneckDto> bottleneckHistory)
    {
        _activeObstructions = activeObstructions;
        _bottleneckHistory = bottleneckHistory;
        _isLoading = false;
        _loadError = null;
        NotifyStateChanged();
    }

    /// <summary>Records a load failure with a sanitized message.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetLoadError(string sanitizedError) { _loadError = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    // ── Egress Simulation Comparison dashboard ──────────────────────────────

    /// <summary>Gets the zone currently selected for the Egress Simulation Comparison dashboard, if any.</summary>
    public Guid? ComparisonZoneId => _comparisonZoneId;

    /// <summary>Gets the modeled (planned) evacuation routes for the selected zone.</summary>
    public IReadOnlyList<EvacuationRouteDto> ComparisonPlannedRoutes => _comparisonPlannedRoutes;

    /// <summary>Gets the most recent (drill-actual) egress simulation for the selected zone, if any.</summary>
    public EgressSimulationDto? ComparisonLatestSimulation => _comparisonLatestSimulation;

    /// <summary>Gets a value indicating whether the comparison dashboard is loading.</summary>
    public bool IsLoadingComparison => _isLoadingComparison;

    /// <summary>Gets a sanitized comparison load error, or null.</summary>
    public string? ComparisonError => _comparisonError;

    /// <summary>Marks the comparison dashboard as loading for the given zone.</summary>
    /// <param name="zoneId">The evacuation zone identifier being compared.</param>
    public void BeginComparisonLoad(Guid zoneId) { _comparisonZoneId = zoneId; _isLoadingComparison = true; _comparisonError = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded planned-routes and latest-simulation data for the comparison dashboard.</summary>
    /// <param name="plannedRoutes">Modeled evacuation routes for the zone (planned egress times).</param>
    /// <param name="latestSimulation">The most recent egress simulation for the zone (drill-actual), or null if none recorded.</param>
    public void SetComparisonData(IReadOnlyList<EvacuationRouteDto> plannedRoutes, EgressSimulationDto? latestSimulation)
    {
        _comparisonPlannedRoutes = plannedRoutes;
        _comparisonLatestSimulation = latestSimulation;
        _isLoadingComparison = false;
        _comparisonError = null;
        NotifyStateChanged();
    }

    /// <summary>Records a comparison-dashboard load failure with a sanitized message.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetComparisonError(string sanitizedError) { _comparisonError = sanitizedError; _isLoadingComparison = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
