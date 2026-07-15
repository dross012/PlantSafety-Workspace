// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Routing Detail page
//   (Volume R1). Holds routing metadata + per-panel loading flags for
//   Steps/Versions/Diff/Simulation/Approval (each loaded on its own
//   async method, matching the Volume R1 state container shape).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Modules.Routing.Models;

namespace PlantSafety.UI.Modules.Routing.State;

/// <summary>Canonical Routing Detail tab identifiers, in display order.</summary>
public enum RoutingDetailTab
{
    /// <summary>Steps tab (default).</summary>
    Steps = 0,

    /// <summary>Versions tab.</summary>
    Versions = 1,

    /// <summary>Diff tab.</summary>
    Diff = 2,

    /// <summary>Simulation tab.</summary>
    Simulation = 3,

    /// <summary>Approval tab.</summary>
    Approval = 4,
}

/// <summary>Singleton state container for the Routing Detail page.</summary>
public sealed class RoutingDetailState
{
    private RoutingDetailDto? _routing;
    private IReadOnlyList<RoutingStepDto> _steps = [];
    private IReadOnlyList<RoutingVersionDto> _versions = [];
    private IReadOnlyList<RoutingDiffRowDto> _diff = [];
    private RoutingSimulationResultDto? _simulation;
    private RoutingApprovalWorkflowDto? _approval;
    private RoutingDetailTab _activeTab = RoutingDetailTab.Steps;
    private bool _isLoadingRouting;
    private bool _isLoadingTab;
    private string? _errorMessage;

    /// <summary>Gets the loaded routing, or null when not yet loaded.</summary>
    public RoutingDetailDto? Routing => _routing;

    /// <summary>Gets the ordered step list.</summary>
    public IReadOnlyList<RoutingStepDto> Steps => _steps;

    /// <summary>Gets the version history timeline.</summary>
    public IReadOnlyList<RoutingVersionDto> Versions => _versions;

    /// <summary>Gets the server-computed diff rows for the currently selected version pair.</summary>
    public IReadOnlyList<RoutingDiffRowDto> Diff => _diff;

    /// <summary>Gets the last simulation result, or null when not yet run.</summary>
    public RoutingSimulationResultDto? Simulation => _simulation;

    /// <summary>Gets the current approval workflow state, or null when not yet loaded.</summary>
    public RoutingApprovalWorkflowDto? Approval => _approval;

    /// <summary>Gets the currently active tab.</summary>
    public RoutingDetailTab ActiveTab => _activeTab;

    /// <summary>Gets a value indicating whether the routing header is loading.</summary>
    public bool IsLoadingRouting => _isLoadingRouting;

    /// <summary>Gets a value indicating whether the active tab is loading.</summary>
    public bool IsLoadingTab => _isLoadingTab;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? ErrorMessage => _errorMessage;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the routing header as loading.</summary>
    public void BeginLoad() { _isLoadingRouting = true; _errorMessage = null; NotifyStateChanged(); }

    /// <summary>Applies the loaded routing header.</summary>
    public void SetRouting(RoutingDetailDto routing) { _routing = routing; _isLoadingRouting = false; _errorMessage = null; NotifyStateChanged(); }

    /// <summary>Marks the active tab as loading.</summary>
    public void BeginTabLoad() { _isLoadingTab = true; NotifyStateChanged(); }

    /// <summary>Applies loaded steps, reordered deterministically by step number.</summary>
    public void SetSteps(IReadOnlyList<RoutingStepDto> steps) { _steps = [.. steps.OrderBy(s => s.StepNumber)]; _isLoadingTab = false; NotifyStateChanged(); }

    /// <summary>Applies loaded version history.</summary>
    public void SetVersions(IReadOnlyList<RoutingVersionDto> versions) { _versions = versions; _isLoadingTab = false; NotifyStateChanged(); }

    /// <summary>Applies the server-computed diff rows.</summary>
    public void SetDiff(IReadOnlyList<RoutingDiffRowDto> diff) { _diff = diff; _isLoadingTab = false; NotifyStateChanged(); }

    /// <summary>Applies a fresh simulation result.</summary>
    public void SetSimulation(RoutingSimulationResultDto simulation) { _simulation = simulation; _isLoadingTab = false; NotifyStateChanged(); }

    /// <summary>Applies the loaded approval workflow state.</summary>
    public void SetApproval(RoutingApprovalWorkflowDto approval) { _approval = approval; _isLoadingTab = false; NotifyStateChanged(); }

    /// <summary>Switches the active tab.</summary>
    public void SetActiveTab(RoutingDetailTab tab) { _activeTab = tab; NotifyStateChanged(); }

    /// <summary>Records a load failure with a sanitized message.</summary>
    public void SetError(string sanitizedError) { _errorMessage = sanitizedError; _isLoadingRouting = false; _isLoadingTab = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
