// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-15
// FILE DESCRIPTION: Singleton state container for the Process Management
//   module. Holds the current page of process definitions and loading/error
//   state. Components dispatch to this state via ProcessManagementApiClient.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Modules.ProcessManagement.Models;

namespace PlantSafety.UI.Modules.ProcessManagement.State;

/// <summary>Singleton state container for the Process Management module.</summary>
public sealed class ProcessManagementState
{
    private IReadOnlyList<ProcessDefinitionDto> _items = [];
    private bool _isLoadingList;
    private string? _listError;
    private int _currentPage = 1;
    private int _totalCount;
    private int _totalPages;

    /// <summary>Gets the current page of Process Definition list items. Never null; empty when not loaded.</summary>
    public IReadOnlyList<ProcessDefinitionDto> Items => _items;

    /// <summary>Gets a value indicating whether the list is loading.</summary>
    public bool IsLoadingList => _isLoadingList;

    /// <summary>Gets a sanitized list load error, or null.</summary>
    public string? ListError => _listError;

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

    /// <summary>Applies a freshly loaded page of Process Definitions.</summary>
    /// <param name="page">The paginated response from the API.</param>
    public void SetItems(ProcessListResponseDto page)
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

    private void NotifyStateChanged() => OnChange?.Invoke();
}
