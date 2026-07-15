// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Crisis Management
//   module. Holds the current page of crisis events, the selected event, and
//   loading/error state.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;

using PlantSafety.UI.Models.Common;
using PlantSafety.UI.Modules.CrisisManagement.Models;

namespace PlantSafety.UI.Modules.CrisisManagement.State;

/// <summary>Singleton state container for the Crisis Management module.</summary>
public sealed class CrisisManagementState
{
    private IReadOnlyList<CrisisEventDto> _items = [];
    private CrisisEventDto? _selected;
    private bool _isLoadingList;
    private bool _isLoadingDetail;
    private string? _listError;
    private string? _detailError;
    private int _currentPage = 1;
    private int _totalCount;
    private int _totalPages;

    /// <summary>Gets the current page of crisis event list items. Never null; empty when not loaded.</summary>
    public IReadOnlyList<CrisisEventDto> Items => _items;

    /// <summary>Gets the count of crisis events currently in the Active status.</summary>
    public int ActiveCount => _items.Count(i => string.Equals(i.Status, "Active", StringComparison.OrdinalIgnoreCase));

    /// <summary>Gets the count of crisis events currently in the Contained status.</summary>
    public int ContainedCount => _items.Count(i => string.Equals(i.Status, "Contained", StringComparison.OrdinalIgnoreCase));

    /// <summary>Gets the count of crisis events currently Resolved or ClosedOut.</summary>
    public int ResolvedCount => _items.Count(i =>
        string.Equals(i.Status, "Resolved", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(i.Status, "ClosedOut", StringComparison.OrdinalIgnoreCase));

    /// <summary>Gets the currently selected crisis event detail, or null when none is selected.</summary>
    public CrisisEventDto? Selected => _selected;

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

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the list as loading.</summary>
    public void BeginListLoad() { _isLoadingList = true; _listError = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded page of crisis events.</summary>
    /// <param name="page">The paginated response from the API.</param>
    public void SetItems(PaginatedResponseDto<CrisisEventDto> page)
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

    /// <summary>Applies the loaded crisis event detail record.</summary>
    /// <param name="item">The full crisis event DTO from the API.</param>
    public void SetSelected(CrisisEventDto item) { _selected = item; _isLoadingDetail = false; _detailError = null; NotifyStateChanged(); }

    /// <summary>Records a detail load failure.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetDetailError(string sanitizedError) { _detailError = sanitizedError; _isLoadingDetail = false; NotifyStateChanged(); }

    /// <summary>Clears the selected crisis event.</summary>
    public void ClearSelection() { _selected = null; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
