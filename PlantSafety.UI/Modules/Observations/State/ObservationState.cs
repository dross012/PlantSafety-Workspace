// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Observation module.
//   Holds the current page of observation records, the selected observation,
//   and loading/error state.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Models.Common;
using PlantSafety.UI.Modules.Observations.Models;

namespace PlantSafety.UI.Modules.Observations.State;

/// <summary>Singleton state container for the Observation module.</summary>
public sealed class ObservationState
{
    private IReadOnlyList<ObservationDto> _items = [];
    private ObservationDto? _selected;
    private bool _isLoadingList;
    private bool _isLoadingDetail;
    private string? _listError;
    private string? _detailError;
    private int _currentPage = 1;
    private int _totalCount;
    private int _totalPages;

    private bool _isCreating;
    private string? _createError;
    private Guid? _createdId;

    private bool _isSaving;
    private string? _saveError;

    /// <summary>Gets the current page of observation list items. Never null; empty when not loaded.</summary>
    public IReadOnlyList<ObservationDto> Items => _items;

    /// <summary>Gets the currently selected observation detail, or null when none is selected.</summary>
    public ObservationDto? Selected => _selected;

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

    /// <summary>Gets a value indicating whether a Create Observation submission is in flight.</summary>
    public bool IsCreating => _isCreating;

    /// <summary>Gets a sanitized Create Observation submission error, or null.</summary>
    public string? CreateError => _createError;

    /// <summary>Gets the identifier of the most recently created observation, or null.</summary>
    public Guid? CreatedId => _createdId;

    /// <summary>Gets a value indicating whether an Edit Observation submission is in flight.</summary>
    public bool IsSaving => _isSaving;

    /// <summary>Gets a sanitized Edit Observation submission error, or null.</summary>
    public string? SaveError => _saveError;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the list as loading.</summary>
    public void BeginListLoad() { _isLoadingList = true; _listError = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded page of observation records.</summary>
    /// <param name="page">The paginated response from the API.</param>
    public void SetItems(PaginatedResponseDto<ObservationDto> page)
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

    /// <summary>Applies the loaded observation detail record.</summary>
    /// <param name="item">The full observation DTO from the API.</param>
    public void SetSelected(ObservationDto item) { _selected = item; _isLoadingDetail = false; _detailError = null; NotifyStateChanged(); }

    /// <summary>Records a detail load failure.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetDetailError(string sanitizedError) { _detailError = sanitizedError; _isLoadingDetail = false; NotifyStateChanged(); }

    /// <summary>Clears the selected observation.</summary>
    public void ClearSelection() { _selected = null; NotifyStateChanged(); }

    /// <summary>Marks a Create Observation submission as in flight.</summary>
    public void BeginCreate() { _isCreating = true; _createError = null; _createdId = null; NotifyStateChanged(); }

    /// <summary>Records a successful Create Observation submission.</summary>
    public void SetCreateSuccess(Guid id) { _createdId = id; _isCreating = false; _createError = null; NotifyStateChanged(); }

    /// <summary>Records a Create Observation submission failure with a sanitized message.</summary>
    public void SetCreateError(string sanitizedError) { _createError = sanitizedError; _isCreating = false; NotifyStateChanged(); }

    /// <summary>Resets Create Observation submission state.</summary>
    public void ResetCreate() { _isCreating = false; _createError = null; _createdId = null; NotifyStateChanged(); }

    /// <summary>Marks an Edit Observation submission as in flight.</summary>
    public void BeginSave() { _isSaving = true; _saveError = null; NotifyStateChanged(); }

    /// <summary>Records a successful Edit Observation submission.</summary>
    public void SetSaveSuccess() { _isSaving = false; _saveError = null; NotifyStateChanged(); }

    /// <summary>Records an Edit Observation submission failure with a sanitized message.</summary>
    public void SetSaveError(string sanitizedError) { _saveError = sanitizedError; _isSaving = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
