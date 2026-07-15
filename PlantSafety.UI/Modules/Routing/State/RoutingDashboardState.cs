// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Routing Dashboard
//   page (Volume R1). Holds the paginated routing list and search/filter
//   text.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Models.Common;
using PlantSafety.UI.Modules.Routing.Models;

namespace PlantSafety.UI.Modules.Routing.State;

/// <summary>Singleton state container for the Routing Dashboard.</summary>
public sealed class RoutingDashboardState
{
    private IReadOnlyList<RoutingSummaryDto> _routings = [];
    private string _searchText = string.Empty;
    private bool _isLoading;
    private string? _errorMessage;
    private int _currentPage = 1;
    private int _totalCount;
    private int _totalPages;
    private DateTimeOffset _lastRefreshUtc;

    /// <summary>Gets the current page of routings.</summary>
    public IReadOnlyList<RoutingSummaryDto> Routings => _routings;

    /// <summary>Gets the current search text.</summary>
    public string SearchText => _searchText;

    /// <summary>Gets a value indicating whether the dashboard is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? ErrorMessage => _errorMessage;

    /// <summary>Gets the current page number.</summary>
    public int CurrentPage => _currentPage;

    /// <summary>Gets the total matching count.</summary>
    public int TotalCount => _totalCount;

    /// <summary>Gets the total page count.</summary>
    public int TotalPages => _totalPages;

    /// <summary>Gets the UTC timestamp of the last successful refresh.</summary>
    public DateTimeOffset LastRefreshUtc => _lastRefreshUtc;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the dashboard as loading.</summary>
    public void BeginLoad() { _isLoading = true; _errorMessage = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded page of routings.</summary>
    public void SetRoutings(PaginatedResponseDto<RoutingSummaryDto> page)
    {
        _routings = page.Items;
        _currentPage = page.PageNumber;
        _totalCount = page.TotalCount;
        _totalPages = page.TotalPages;
        _isLoading = false;
        _errorMessage = null;
        _lastRefreshUtc = DateTimeOffset.UtcNow;
        NotifyStateChanged();
    }

    /// <summary>Applies a new search filter.</summary>
    public void SetSearchText(string search) { _searchText = search; NotifyStateChanged(); }

    /// <summary>Records a load failure with a sanitized message.</summary>
    public void SetError(string sanitizedError) { _errorMessage = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
