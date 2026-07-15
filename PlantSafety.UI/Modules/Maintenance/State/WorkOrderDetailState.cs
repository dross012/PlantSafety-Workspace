// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Work Order Detail
//   page (Volume M1). Holds summary/metadata, Tasks/Parts/History/
//   Predictive/Metadata tab content, and the active tab (lazy-loaded).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Modules.Maintenance.Models;

namespace PlantSafety.UI.Modules.Maintenance.State;

/// <summary>Canonical Work Order Detail tab identifiers, in display order.</summary>
public enum WorkOrderDetailTab
{
    /// <summary>Tasks tab (default).</summary>
    Tasks = 0,

    /// <summary>Parts tab.</summary>
    Parts = 1,

    /// <summary>History tab.</summary>
    History = 2,

    /// <summary>Predictive insights tab.</summary>
    Predictive = 3,

    /// <summary>Metadata tab.</summary>
    Metadata = 4,
}

/// <summary>Singleton state container for the Work Order Detail page.</summary>
public sealed class WorkOrderDetailState
{
    private WorkOrderDetailDto? _workOrder;
    private IReadOnlyList<TaskStepDto> _tasks = [];
    private IReadOnlyList<PartUsageDto> _parts = [];
    private IReadOnlyList<MaintenanceEventDto> _history = [];
    private MaintenancePredictiveInsightsDto? _predictive;
    private WorkOrderDetailTab _activeTab = WorkOrderDetailTab.Tasks;
    private bool _isLoading;
    private bool _isLoadingTab;
    private string? _errorMessage;

    /// <summary>Gets the loaded work order, or null when not yet loaded.</summary>
    public WorkOrderDetailDto? WorkOrder => _workOrder;

    /// <summary>Gets the ordered task steps.</summary>
    public IReadOnlyList<TaskStepDto> Tasks => _tasks;

    /// <summary>Gets the parts used.</summary>
    public IReadOnlyList<PartUsageDto> Parts => _parts;

    /// <summary>Gets the chronological event history.</summary>
    public IReadOnlyList<MaintenanceEventDto> History => _history;

    /// <summary>Gets the predictive insights, or null when not yet loaded.</summary>
    public MaintenancePredictiveInsightsDto? Predictive => _predictive;

    /// <summary>Gets the currently active tab.</summary>
    public WorkOrderDetailTab ActiveTab => _activeTab;

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

    /// <summary>Applies the loaded work order header.</summary>
    public void SetWorkOrder(WorkOrderDetailDto workOrder) { _workOrder = workOrder; _isLoading = false; _errorMessage = null; NotifyStateChanged(); }

    /// <summary>Marks the active tab as loading.</summary>
    public void BeginTabLoad() { _isLoadingTab = true; NotifyStateChanged(); }

    /// <summary>Applies loaded task steps, reordering deterministically if the caller passes an unordered list.</summary>
    public void SetTasks(IReadOnlyList<TaskStepDto> tasks) { _tasks = [.. tasks.OrderBy(t => t.StepNumber)]; _isLoadingTab = false; NotifyStateChanged(); }

    /// <summary>Applies loaded parts.</summary>
    public void SetParts(IReadOnlyList<PartUsageDto> parts) { _parts = parts; _isLoadingTab = false; NotifyStateChanged(); }

    /// <summary>Applies loaded history.</summary>
    public void SetHistory(IReadOnlyList<MaintenanceEventDto> history) { _history = history; _isLoadingTab = false; NotifyStateChanged(); }

    /// <summary>Applies freshly generated predictive insights.</summary>
    public void SetPredictive(MaintenancePredictiveInsightsDto predictive) { _predictive = predictive; _isLoadingTab = false; NotifyStateChanged(); }

    /// <summary>Switches the active tab.</summary>
    public void SetActiveTab(WorkOrderDetailTab tab) { _activeTab = tab; NotifyStateChanged(); }

    /// <summary>Records a load failure with a sanitized message.</summary>
    public void SetError(string sanitizedError) { _errorMessage = sanitizedError; _isLoading = false; _isLoadingTab = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
