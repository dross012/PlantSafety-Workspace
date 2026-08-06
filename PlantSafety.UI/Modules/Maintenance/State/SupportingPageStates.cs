// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state containers for the Preventive Maintenance
//   Schedule, Technician Workload, Parts Usage & Inventory, and Maintenance
//   Calendar pages (Volume M1 high-fidelity notes + Volume MC1).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Models.Common;
using PlantSafety.UI.Modules.Maintenance.Models;

namespace PlantSafety.UI.Modules.Maintenance.State;

/// <summary>Singleton state container for the Preventive Maintenance Schedule page.</summary>
public sealed class PreventiveScheduleState
{
    private IReadOnlyList<PreventiveTaskDto> _tasks = [];
    private bool _isLoading;
    private string? _errorMessage;

    /// <summary>Gets the preventive tasks currently loaded.</summary>
    public IReadOnlyList<PreventiveTaskDto> Tasks => _tasks;

    /// <summary>Gets a value indicating whether the page is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? ErrorMessage => _errorMessage;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the page as loading.</summary>
    public void BeginLoad() { _isLoading = true; _errorMessage = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded page of preventive tasks.</summary>
    public void SetTasks(PaginatedResponseDto<PreventiveTaskDto> page)
    {
        _tasks = page.Items;
        _isLoading = false;
        _errorMessage = null;
        NotifyStateChanged();
    }

    /// <summary>Records a load failure with a sanitized message.</summary>
    public void SetError(string sanitizedError) { _errorMessage = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}

/// <summary>Singleton state container for the Technician Workload View page.</summary>
public sealed class TechnicianWorkloadState
{
    private IReadOnlyList<TechnicianWorkloadDto> _technicians = [];
    private bool _isLoading;
    private string? _errorMessage;

    /// <summary>Gets the technician workload cards currently loaded.</summary>
    public IReadOnlyList<TechnicianWorkloadDto> Technicians => _technicians;

    /// <summary>Gets a value indicating whether the page is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? ErrorMessage => _errorMessage;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the page as loading.</summary>
    public void BeginLoad() { _isLoading = true; _errorMessage = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded page of technician workload cards.</summary>
    public void SetTechnicians(PaginatedResponseDto<TechnicianWorkloadDto> page)
    {
        _technicians = page.Items;
        _isLoading = false;
        _errorMessage = null;
        NotifyStateChanged();
    }

    /// <summary>Records a load failure with a sanitized message.</summary>
    public void SetError(string sanitizedError) { _errorMessage = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}

/// <summary>Singleton state container for the Parts Usage &amp; Inventory View page.</summary>
public sealed class PartsInventoryState
{
    private IReadOnlyList<PartInventoryDto> _parts = [];
    private bool _isLoading;
    private string? _errorMessage;
    private bool _isAdjustingStock;
    private string? _adjustStockError;

    /// <summary>Gets the parts inventory rows currently loaded.</summary>
    public IReadOnlyList<PartInventoryDto> Parts => _parts;

    /// <summary>Gets a value indicating whether the page is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? ErrorMessage => _errorMessage;

    /// <summary>Gets a value indicating whether a stock adjustment submission is in flight.</summary>
    public bool IsAdjustingStock => _isAdjustingStock;

    /// <summary>Gets a sanitized stock adjustment submission error, or null.</summary>
    public string? AdjustStockError => _adjustStockError;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the page as loading.</summary>
    public void BeginLoad() { _isLoading = true; _errorMessage = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded page of inventory rows.</summary>
    public void SetParts(PaginatedResponseDto<PartInventoryDto> page)
    {
        _parts = page.Items;
        _isLoading = false;
        _errorMessage = null;
        NotifyStateChanged();
    }

    /// <summary>Records a load failure with a sanitized message.</summary>
    public void SetError(string sanitizedError) { _errorMessage = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    /// <summary>Marks a stock adjustment submission as in flight.</summary>
    public void BeginAdjustStock() { _isAdjustingStock = true; _adjustStockError = null; NotifyStateChanged(); }

    /// <summary>Records a successful stock adjustment submission.</summary>
    public void SetAdjustStockSuccess() { _isAdjustingStock = false; _adjustStockError = null; NotifyStateChanged(); }

    /// <summary>Records a stock adjustment submission failure with a sanitized message.</summary>
    public void SetAdjustStockError(string sanitizedError) { _adjustStockError = sanitizedError; _isAdjustingStock = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}

/// <summary>Singleton state container for the Maintenance Calendar page (Volume MC1).</summary>
public sealed class MaintenanceCalendarState
{
    private IReadOnlyList<MaintenanceCalendarEventDto> _events = [];
    private DateOnly _rangeStart = DateOnly.FromDateTime(DateTime.UtcNow);
    private DateOnly _rangeEnd = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(7);
    private bool _isLoading;
    private string? _errorMessage;

    /// <summary>Gets the calendar events currently loaded.</summary>
    public IReadOnlyList<MaintenanceCalendarEventDto> Events => _events;

    /// <summary>Gets the current visible range start date.</summary>
    public DateOnly RangeStart => _rangeStart;

    /// <summary>Gets the current visible range end date.</summary>
    public DateOnly RangeEnd => _rangeEnd;

    /// <summary>Gets a value indicating whether the calendar is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? ErrorMessage => _errorMessage;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the calendar as loading.</summary>
    public void BeginLoad() { _isLoading = true; _errorMessage = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded page of calendar events for the given range.</summary>
    public void SetEvents(DateOnly rangeStart, DateOnly rangeEnd, PaginatedResponseDto<MaintenanceCalendarEventDto> page)
    {
        _rangeStart = rangeStart;
        _rangeEnd = rangeEnd;
        _events = page.Items;
        _isLoading = false;
        _errorMessage = null;
        NotifyStateChanged();
    }

    /// <summary>Records a load failure with a sanitized message.</summary>
    public void SetError(string sanitizedError) { _errorMessage = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
