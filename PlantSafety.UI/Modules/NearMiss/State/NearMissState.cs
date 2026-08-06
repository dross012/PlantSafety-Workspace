// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Near Miss module. Holds
//   the current paginated near miss list, the selected near miss, and
//   loading/error state. Components never call HttpClient directly — they
//   dispatch to this state via NearMissApiClient.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Models.Common;
using PlantSafety.UI.Modules.NearMiss.Models;

namespace PlantSafety.UI.Modules.NearMiss.State;

/// <summary>Singleton state container for the Near Miss module.</summary>
public sealed class NearMissState
{
    private IReadOnlyList<NearMissDto> _items = [];
    private NearMissDto? _selected;
    private bool _isLoadingList;
    private bool _isLoadingDetail;
    private string? _listError;
    private string? _detailError;
    private int _currentPage = 1;
    private int _totalCount;
    private int _totalPages;

    private string? _potentialSeverityFilter;
    private string? _hazardCategoryFilter;
    private DateOnly _startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2));
    private DateOnly _endDate = DateOnly.FromDateTime(DateTime.UtcNow);

    private bool _isCreating;
    private string? _createError;
    private Guid? _createdId;

    private bool _isSaving;
    private string? _saveError;

    /// <summary>Gets the current page of near miss list items. Never null; empty when not loaded.</summary>
    public IReadOnlyList<NearMissDto> Items => _items;

    /// <summary>Gets the currently selected near miss detail, or null when none is selected.</summary>
    public NearMissDto? Selected => _selected;

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

    /// <summary>Gets the current server-side potential-severity filter, or null for "All".</summary>
    public string? PotentialSeverityFilter => _potentialSeverityFilter;

    /// <summary>Gets the current server-side hazard-category filter, or null for "All".</summary>
    public string? HazardCategoryFilter => _hazardCategoryFilter;

    /// <summary>Gets the inclusive start of the loaded occurrence-date range.</summary>
    public DateOnly StartDate => _startDate;

    /// <summary>Gets the inclusive end of the loaded occurrence-date range.</summary>
    public DateOnly EndDate => _endDate;

    /// <summary>Gets a value indicating whether a Create Near Miss submission is in flight.</summary>
    public bool IsCreating => _isCreating;

    /// <summary>Gets a sanitized Create Near Miss submission error, or null.</summary>
    public string? CreateError => _createError;

    /// <summary>Gets the identifier of the most recently created near miss, or null.</summary>
    public Guid? CreatedId => _createdId;

    /// <summary>Gets a value indicating whether an Edit Near Miss submission is in flight.</summary>
    public bool IsSaving => _isSaving;

    /// <summary>Gets a sanitized Edit Near Miss submission error, or null.</summary>
    public string? SaveError => _saveError;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the list as loading.</summary>
    public void BeginListLoad() { _isLoadingList = true; _listError = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded page of near miss records.</summary>
    /// <param name="page">The paginated response from the API.</param>
    public void SetItems(PaginatedResponseDto<NearMissDto> page)
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

    /// <summary>Applies the loaded near miss detail record.</summary>
    /// <param name="item">The full near miss DTO from the API.</param>
    public void SetSelected(NearMissDto item) { _selected = item; _isLoadingDetail = false; _detailError = null; NotifyStateChanged(); }

    /// <summary>Records a detail load failure.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetDetailError(string sanitizedError) { _detailError = sanitizedError; _isLoadingDetail = false; NotifyStateChanged(); }

    /// <summary>Clears the selected near miss.</summary>
    public void ClearSelection() { _selected = null; NotifyStateChanged(); }

    /// <summary>Updates the server-side severity/hazard-category filters.</summary>
    public void SetFilters(string? potentialSeverity, string? hazardCategory)
    {
        _potentialSeverityFilter = potentialSeverity;
        _hazardCategoryFilter = hazardCategory;
        NotifyStateChanged();
    }

    /// <summary>Updates the loaded occurrence-date range. Callers MUST re-fetch after calling this.</summary>
    public void SetDateRange(DateOnly start, DateOnly end) { _startDate = start; _endDate = end; NotifyStateChanged(); }

    /// <summary>Marks a Create Near Miss submission as in flight.</summary>
    public void BeginCreate() { _isCreating = true; _createError = null; _createdId = null; NotifyStateChanged(); }

    /// <summary>Records a successful Create Near Miss submission.</summary>
    public void SetCreateSuccess(Guid id) { _createdId = id; _isCreating = false; _createError = null; NotifyStateChanged(); }

    /// <summary>Records a Create Near Miss submission failure with a sanitized message.</summary>
    public void SetCreateError(string sanitizedError) { _createError = sanitizedError; _isCreating = false; NotifyStateChanged(); }

    /// <summary>Resets Create Near Miss submission state.</summary>
    public void ResetCreate() { _isCreating = false; _createError = null; _createdId = null; NotifyStateChanged(); }

    /// <summary>Marks an Edit Near Miss submission as in flight.</summary>
    public void BeginSave() { _isSaving = true; _saveError = null; NotifyStateChanged(); }

    /// <summary>Records a successful Edit Near Miss submission.</summary>
    public void SetSaveSuccess() { _isSaving = false; _saveError = null; NotifyStateChanged(); }

    /// <summary>Records an Edit Near Miss submission failure with a sanitized message.</summary>
    public void SetSaveError(string sanitizedError) { _saveError = sanitizedError; _isSaving = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
