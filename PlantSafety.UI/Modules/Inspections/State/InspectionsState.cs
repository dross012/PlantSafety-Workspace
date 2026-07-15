// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Inspections module.
//   Holds the full date-range-filtered record set loaded from the API,
//   derives client-side search filtering and pagination (the read-model
//   endpoint has no server-side paging), the selected detail record, and
//   loading/error state. Components never call HttpClient directly — they
//   dispatch to this state via the InspectionsApiClient, which is injected
//   by components that trigger loads.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;

using PlantSafety.UI.Modules.Inspections.Models;

namespace PlantSafety.UI.Modules.Inspections.State;

/// <summary>
/// Singleton state container for the Inspections module list and detail views.
/// </summary>
public sealed class InspectionsState
{
    private const int PageSize = 25;

    private IReadOnlyList<InspectionDto> _allItems = [];
    private string _searchText = string.Empty;
    private DateOnly _startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-90));
    private DateOnly _endDate = DateOnly.FromDateTime(DateTime.UtcNow);
    private int _currentPage = 1;
    private InspectionDto? _selectedItem;
    private bool _isLoadingList;
    private bool _isLoadingDetail;
    private string? _listError;
    private string? _detailError;

    /// <summary>Gets the full, unfiltered record set for the current date range.</summary>
    public IReadOnlyList<InspectionDto> AllItems => _allItems;

    /// <summary>Gets the current client-side search text.</summary>
    public string SearchText => _searchText;

    /// <summary>Gets the inclusive start of the loaded date range.</summary>
    public DateOnly StartDate => _startDate;

    /// <summary>Gets the inclusive end of the loaded date range.</summary>
    public DateOnly EndDate => _endDate;

    /// <summary>Gets the current one-based page number.</summary>
    public int CurrentPage => _currentPage;

    /// <summary>Gets the currently selected detail record, or null when none is selected.</summary>
    public InspectionDto? SelectedItem => _selectedItem;

    /// <summary>Gets a value indicating whether the list is loading.</summary>
    public bool IsLoadingList => _isLoadingList;

    /// <summary>Gets a value indicating whether a detail record is loading.</summary>
    public bool IsLoadingDetail => _isLoadingDetail;

    /// <summary>Gets a sanitized list load error, or null.</summary>
    public string? ListError => _listError;

    /// <summary>Gets a sanitized detail load error, or null.</summary>
    public string? DetailError => _detailError;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Gets the items matching the current search text (case-insensitive, client-side).</summary>
    public IReadOnlyList<InspectionDto> FilteredItems =>
        string.IsNullOrWhiteSpace(_searchText)
            ? _allItems
            : _allItems.Where(i =>
                    i.InspectionType.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ||
                    i.Outcome.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ||
                    i.Status.Contains(_searchText, StringComparison.OrdinalIgnoreCase))
                .ToList();

    /// <summary>Gets the total number of items matching the current search filter.</summary>
    public int TotalCount => FilteredItems.Count;

    /// <summary>Gets the total number of pages given <see cref="PageSize"/>.</summary>
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));

    /// <summary>Gets the current page slice of <see cref="FilteredItems"/>.</summary>
    public IReadOnlyList<InspectionDto> PagedItems =>
        FilteredItems.Skip((_currentPage - 1) * PageSize).Take(PageSize).ToList();

    /// <summary>Marks the list as loading.</summary>
    public void BeginListLoad() { _isLoadingList = true; _listError = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded, unpaginated record set for the current date range.</summary>
    /// <param name="items">The full set of records returned by the API for the current range.</param>
    public void SetItems(IReadOnlyList<InspectionDto> items)
    {
        _allItems = items;
        _currentPage = 1;
        _isLoadingList = false;
        _listError = null;
        NotifyStateChanged();
    }

    /// <summary>Records a list load failure with a sanitized message.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetListError(string sanitizedError) { _listError = sanitizedError; _isLoadingList = false; NotifyStateChanged(); }

    /// <summary>Updates the client-side search text and resets to page 1.</summary>
    /// <param name="text">The new search text.</param>
    public void SetSearchText(string text) { _searchText = text; _currentPage = 1; NotifyStateChanged(); }

    /// <summary>Updates the loaded date range. Callers MUST re-fetch after calling this.</summary>
    /// <param name="start">Inclusive range start.</param>
    /// <param name="end">Inclusive range end.</param>
    public void SetDateRange(DateOnly start, DateOnly end) { _startDate = start; _endDate = end; NotifyStateChanged(); }

    /// <summary>Moves to the given one-based page within <see cref="TotalPages"/>.</summary>
    /// <param name="page">The requested page number.</param>
    public void SetPage(int page)
    {
        if (page < 1 || page > TotalPages) return;
        _currentPage = page;
        NotifyStateChanged();
    }

    /// <summary>Marks the detail panel as loading.</summary>
    public void BeginDetailLoad() { _isLoadingDetail = true; _detailError = null; NotifyStateChanged(); }

    /// <summary>Applies the resolved detail record (found in-memory or re-fetched).</summary>
    /// <param name="item">The selected record.</param>
    public void SetSelectedItem(InspectionDto item) { _selectedItem = item; _isLoadingDetail = false; _detailError = null; NotifyStateChanged(); }

    /// <summary>Records a detail resolution failure.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetDetailError(string sanitizedError) { _detailError = sanitizedError; _isLoadingDetail = false; NotifyStateChanged(); }

    /// <summary>Clears the selected detail record.</summary>
    public void ClearSelection() { _selectedItem = null; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
