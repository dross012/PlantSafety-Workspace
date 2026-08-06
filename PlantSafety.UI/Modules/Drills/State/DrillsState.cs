// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Drills & Exercises
//   module. Holds the current page of drill records, the selected drill,
//   and loading/error state.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Models.Common;
using PlantSafety.UI.Modules.Drills.Models;

namespace PlantSafety.UI.Modules.Drills.State;

/// <summary>Singleton state container for the Drills &amp; Exercises module.</summary>
public sealed class DrillsState
{
    private IReadOnlyList<DrillDto> _items = [];
    private DrillDto? _selected;
    private bool _isLoadingList;
    private bool _isLoadingDetail;
    private string? _listError;
    private string? _detailError;
    private int _currentPage = 1;
    private int _totalCount;
    private int _totalPages;
    private bool _isCreating;
    private string? _createError;
    private Guid? _createdDrillId;
    private string? _statusFilter;
    private string? _drillTypeFilter;

    /// <summary>Gets the current page of drill list items. Never null; empty when not loaded.</summary>
    public IReadOnlyList<DrillDto> Items => _items;

    /// <summary>Gets the currently selected drill detail, or null when none is selected.</summary>
    public DrillDto? Selected => _selected;

    /// <summary>Gets a value indicating whether the list is loading.</summary>
    public bool IsLoadingList => _isLoadingList;

    /// <summary>Gets a value indicating whether a detail record is loading.</summary>
    public bool IsLoadingDetail => _isLoadingDetail;

    /// <summary>Gets a sanitized list load error, or null.</summary>
    public string? ListError => _listError;

    /// <summary>Gets a sanitized detail load error, or null.</summary>
    public string? DetailError => _detailError;

    /// <summary>Gets the current page number (one-based).</summary>
    public int CurrentPage => _currentPage;

    /// <summary>Gets the total matching record count across all pages.</summary>
    public int TotalCount => _totalCount;

    /// <summary>Gets the total number of pages.</summary>
    public int TotalPages => _totalPages;

    /// <summary>Gets a value indicating whether a Create Drill request is in flight.</summary>
    public bool IsCreating => _isCreating;

    /// <summary>Gets a sanitized Create Drill error, or null.</summary>
    public string? CreateError => _createError;

    /// <summary>Gets the identifier of the most recently created drill, or null.</summary>
    public Guid? CreatedDrillId => _createdDrillId;

    /// <summary>Gets the current status filter applied to the list, or null when unfiltered.</summary>
    public string? StatusFilter => _statusFilter;

    /// <summary>Gets the current drill-type filter applied to the list, or null when unfiltered.</summary>
    public string? DrillTypeFilter => _drillTypeFilter;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Updates the list's status filter.</summary>
    public void SetStatusFilter(string? status) { _statusFilter = string.IsNullOrWhiteSpace(status) ? null : status; NotifyStateChanged(); }

    /// <summary>Updates the list's drill-type filter.</summary>
    public void SetDrillTypeFilter(string? drillType) { _drillTypeFilter = string.IsNullOrWhiteSpace(drillType) ? null : drillType; NotifyStateChanged(); }

    /// <summary>Marks a Create Drill request as in flight.</summary>
    public void BeginCreate() { _isCreating = true; _createError = null; _createdDrillId = null; NotifyStateChanged(); }

    /// <summary>Records a successful Create Drill request.</summary>
    public void SetCreateSuccess(Guid drillId) { _createdDrillId = drillId; _isCreating = false; _createError = null; NotifyStateChanged(); }

    /// <summary>Records a Create Drill failure.</summary>
    public void SetCreateError(string sanitizedError) { _createError = sanitizedError; _isCreating = false; NotifyStateChanged(); }

    /// <summary>Resets Create Drill transient state (call on navigating to the Create page).</summary>
    public void ResetCreate() { _isCreating = false; _createError = null; _createdDrillId = null; NotifyStateChanged(); }

    /// <summary>Marks the list as loading.</summary>
    public void BeginListLoad() { _isLoadingList = true; _listError = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded page of drill records.</summary>
    /// <param name="page">The paginated response from the API.</param>
    public void SetItems(PaginatedResponseDto<DrillDto> page)
    {
        _items = page.Items;
        _currentPage = page.PageNumber;
        _totalCount = page.TotalCount;
        _totalPages = page.TotalPages;
        _isLoadingList = false;
        _listError = null;
        NotifyStateChanged();
    }

    /// <summary>Records a list load failure with a sanitized message.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetListError(string sanitizedError) { _listError = sanitizedError; _isLoadingList = false; NotifyStateChanged(); }

    /// <summary>Marks the detail panel as loading.</summary>
    public void BeginDetailLoad() { _isLoadingDetail = true; _detailError = null; NotifyStateChanged(); }

    /// <summary>Applies the loaded drill detail record.</summary>
    /// <param name="item">The full drill DTO from the API.</param>
    public void SetSelected(DrillDto item) { _selected = item; _isLoadingDetail = false; _detailError = null; NotifyStateChanged(); }

    /// <summary>Records a detail load failure.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetDetailError(string sanitizedError) { _detailError = sanitizedError; _isLoadingDetail = false; NotifyStateChanged(); }

    /// <summary>Clears the selected drill.</summary>
    public void ClearSelection() { _selected = null; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
