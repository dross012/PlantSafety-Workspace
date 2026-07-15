// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Document Management
//   module's list and detail pages, following the generic list/detail
//   template (Alerts pattern) since no dedicated architecture manual
//   exists for this module.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Models.Common;
using PlantSafety.UI.Modules.DocumentManagement.Models;

namespace PlantSafety.UI.Modules.DocumentManagement.State;

/// <summary>Singleton state container for the Document Management module.</summary>
public sealed class DocumentManagementState
{
    private IReadOnlyList<DocumentListItemDto> _documents = [];
    private DocumentDetailDto? _selectedDocument;
    private bool _isLoadingList;
    private bool _isLoadingDetail;
    private string? _listError;
    private string? _detailError;
    private int _currentPage = 1;
    private int _totalCount;
    private int _totalPages;
    private string? _categoryFilter;
    private string _searchText = string.Empty;

    /// <summary>Gets the current page of documents. Never null; empty when not loaded.</summary>
    public IReadOnlyList<DocumentListItemDto> Documents => _documents;

    /// <summary>Gets the currently selected document detail, or null when none is selected.</summary>
    public DocumentDetailDto? SelectedDocument => _selectedDocument;

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

    /// <summary>Gets the total matching document count across all pages.</summary>
    public int TotalCount => _totalCount;

    /// <summary>Gets the total number of pages.</summary>
    public int TotalPages => _totalPages;

    /// <summary>Gets the currently applied category filter, or null for all categories.</summary>
    public string? CategoryFilter => _categoryFilter;

    /// <summary>Gets the current search text.</summary>
    public string SearchText => _searchText;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the list as loading.</summary>
    public void BeginListLoad() { _isLoadingList = true; _listError = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded page of documents.</summary>
    public void SetDocuments(PaginatedResponseDto<DocumentListItemDto> page)
    {
        _documents = page.Items;
        _currentPage = page.PageNumber;
        _totalCount = page.TotalCount;
        _totalPages = page.TotalPages;
        _isLoadingList = false;
        _listError = null;
        NotifyStateChanged();
    }

    /// <summary>Applies a new category filter.</summary>
    public void SetCategoryFilter(string? categoryFilter) { _categoryFilter = categoryFilter; NotifyStateChanged(); }

    /// <summary>Applies a new search filter.</summary>
    public void SetSearchText(string search) { _searchText = search; NotifyStateChanged(); }

    /// <summary>Records a list load failure with a sanitized message.</summary>
    public void SetListError(string sanitizedError) { _listError = sanitizedError; _isLoadingList = false; NotifyStateChanged(); }

    /// <summary>Marks the detail panel as loading.</summary>
    public void BeginDetailLoad() { _isLoadingDetail = true; _detailError = null; NotifyStateChanged(); }

    /// <summary>Applies the loaded document detail record.</summary>
    public void SetSelectedDocument(DocumentDetailDto document) { _selectedDocument = document; _isLoadingDetail = false; _detailError = null; NotifyStateChanged(); }

    /// <summary>Records a detail load failure.</summary>
    public void SetDetailError(string sanitizedError) { _detailError = sanitizedError; _isLoadingDetail = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
