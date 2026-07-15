// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Maintenance Dashboard
//   page (Volume M1). Holds Work Order Summary, Machine Status Summary, and
//   the Active Work Orders page.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Models.Common;
using PlantSafety.UI.Modules.Maintenance.Models;

namespace PlantSafety.UI.Modules.Maintenance.State;

/// <summary>Singleton state container for the Maintenance Dashboard.</summary>
public sealed class MaintenanceDashboardState
{
    private WorkOrderSummaryDto? _workOrderSummary;
    private MachineStatusSummaryDto? _machineStatusSummary;
    private IReadOnlyList<WorkOrderRowDto> _activeWorkOrders = [];
    private bool _isLoading;
    private string? _errorMessage;
    private int _currentPage = 1;
    private int _totalCount;
    private int _totalPages;
    private string? _statusFilter;
    private string? _technicianFilter;

    /// <summary>Gets the Work Order Summary card data, or null when not yet loaded.</summary>
    public WorkOrderSummaryDto? WorkOrderSummary => _workOrderSummary;

    /// <summary>Gets the Machine Status Summary card data, or null when not yet loaded.</summary>
    public MachineStatusSummaryDto? MachineStatusSummary => _machineStatusSummary;

    /// <summary>Gets the current page of active work orders.</summary>
    public IReadOnlyList<WorkOrderRowDto> ActiveWorkOrders => _activeWorkOrders;

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

    /// <summary>Gets the applied status filter.</summary>
    public string? StatusFilter => _statusFilter;

    /// <summary>Gets the applied technician filter.</summary>
    public string? TechnicianFilter => _technicianFilter;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the dashboard as loading.</summary>
    public void BeginLoad() { _isLoading = true; _errorMessage = null; NotifyStateChanged(); }

    /// <summary>Applies the loaded summary cards.</summary>
    public void SetSummaries(WorkOrderSummaryDto woSummary, MachineStatusSummaryDto machineSummary)
    {
        _workOrderSummary = woSummary;
        _machineStatusSummary = machineSummary;
        NotifyStateChanged();
    }

    /// <summary>Applies a freshly loaded page of active work orders. Updates rows without resetting scroll/focus.</summary>
    public void SetActiveWorkOrders(PaginatedResponseDto<WorkOrderRowDto> page)
    {
        _activeWorkOrders = page.Items;
        _currentPage = page.PageNumber;
        _totalCount = page.TotalCount;
        _totalPages = page.TotalPages;
        _isLoading = false;
        _errorMessage = null;
        NotifyStateChanged();
    }

    /// <summary>Applies new filters.</summary>
    public void SetFilters(string? statusFilter, string? technicianFilter)
    {
        _statusFilter = statusFilter;
        _technicianFilter = technicianFilter;
        NotifyStateChanged();
    }

    /// <summary>Records a load failure with a sanitized message.</summary>
    public void SetError(string sanitizedError) { _errorMessage = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
