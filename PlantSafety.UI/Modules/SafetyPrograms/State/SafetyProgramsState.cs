// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Safety Programs
//   module's list and detail pages, following the generic list/detail
//   template (Alerts pattern) since no dedicated architecture manual
//   exists for this module.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Models.Common;
using PlantSafety.UI.Modules.SafetyPrograms.Models;

namespace PlantSafety.UI.Modules.SafetyPrograms.State;

/// <summary>Singleton state container for the Safety Programs module.</summary>
public sealed class SafetyProgramsState
{
    private IReadOnlyList<SafetyProgramListItemDto> _programs = [];
    private SafetyProgramDetailDto? _selectedProgram;
    private bool _isLoadingList;
    private bool _isLoadingDetail;
    private string? _listError;
    private string? _detailError;
    private int _currentPage = 1;
    private int _totalCount;
    private int _totalPages;
    private string? _statusFilter;

    /// <summary>Gets the current page of programs. Never null; empty when not loaded.</summary>
    public IReadOnlyList<SafetyProgramListItemDto> Programs => _programs;

    /// <summary>Gets the currently selected program detail, or null when none is selected.</summary>
    public SafetyProgramDetailDto? SelectedProgram => _selectedProgram;

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

    /// <summary>Gets the total matching program count across all pages.</summary>
    public int TotalCount => _totalCount;

    /// <summary>Gets the total number of pages.</summary>
    public int TotalPages => _totalPages;

    /// <summary>Gets the currently applied status filter, or null for all statuses.</summary>
    public string? StatusFilter => _statusFilter;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the list as loading.</summary>
    public void BeginListLoad() { _isLoadingList = true; _listError = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded page of programs.</summary>
    public void SetPrograms(PaginatedResponseDto<SafetyProgramListItemDto> page)
    {
        _programs = page.Items;
        _currentPage = page.PageNumber;
        _totalCount = page.TotalCount;
        _totalPages = page.TotalPages;
        _isLoadingList = false;
        _listError = null;
        NotifyStateChanged();
    }

    /// <summary>Applies a new status filter.</summary>
    public void SetStatusFilter(string? statusFilter) { _statusFilter = statusFilter; NotifyStateChanged(); }

    /// <summary>Records a list load failure with a sanitized message.</summary>
    public void SetListError(string sanitizedError) { _listError = sanitizedError; _isLoadingList = false; NotifyStateChanged(); }

    /// <summary>Marks the detail panel as loading.</summary>
    public void BeginDetailLoad() { _isLoadingDetail = true; _detailError = null; NotifyStateChanged(); }

    /// <summary>Applies the loaded program detail record.</summary>
    public void SetSelectedProgram(SafetyProgramDetailDto program) { _selectedProgram = program; _isLoadingDetail = false; _detailError = null; NotifyStateChanged(); }

    /// <summary>Records a detail load failure.</summary>
    public void SetDetailError(string sanitizedError) { _detailError = sanitizedError; _isLoadingDetail = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
