// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Incidents List page.
//   Follows the generic list template (Alerts pattern).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Models.Common;
using PlantSafety.UI.Modules.IncidentManagement.Models;

namespace PlantSafety.UI.Modules.IncidentManagement.State;

/// <summary>Singleton state container for the Incidents List page.</summary>
public sealed class IncidentsListState
{
    private IReadOnlyList<IncidentSummaryDto> _incidents = [];
    private bool _isLoading;
    private string? _errorMessage;
    private int _currentPage = 1;
    private int _totalCount;
    private int _totalPages;
    private string? _severityFilter;
    private string? _statusFilter;

    /// <summary>Gets the current page of incidents.</summary>
    public IReadOnlyList<IncidentSummaryDto> Incidents => _incidents;

    /// <summary>Gets a value indicating whether the list is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? ErrorMessage => _errorMessage;

    /// <summary>Gets the current page number.</summary>
    public int CurrentPage => _currentPage;

    /// <summary>Gets the total matching count.</summary>
    public int TotalCount => _totalCount;

    /// <summary>Gets the total page count.</summary>
    public int TotalPages => _totalPages;

    /// <summary>Gets the applied severity filter.</summary>
    public string? SeverityFilter => _severityFilter;

    /// <summary>Gets the applied status filter.</summary>
    public string? StatusFilter => _statusFilter;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the list as loading.</summary>
    public void BeginLoad() { _isLoading = true; _errorMessage = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded page of incidents.</summary>
    public void SetIncidents(PaginatedResponseDto<IncidentSummaryDto> page)
    {
        _incidents = page.Items;
        _currentPage = page.PageNumber;
        _totalCount = page.TotalCount;
        _totalPages = page.TotalPages;
        _isLoading = false;
        _errorMessage = null;
        NotifyStateChanged();
    }

    /// <summary>Applies new filters.</summary>
    public void SetFilters(string? severityFilter, string? statusFilter)
    {
        _severityFilter = severityFilter;
        _statusFilter = statusFilter;
        NotifyStateChanged();
    }

    /// <summary>Records a load failure with a sanitized message.</summary>
    public void SetError(string sanitizedError) { _errorMessage = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
