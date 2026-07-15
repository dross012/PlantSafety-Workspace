// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state containers for the Batch Tracking,
//   Production Timeline, KPI Dashboard, and Production Event History pages
//   (Volume T1 §3/§4/§5/§6).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Models.Common;
using PlantSafety.UI.Modules.ProductionTracking.Models;

namespace PlantSafety.UI.Modules.ProductionTracking.State;

/// <summary>Singleton state container for the Batch Tracking View page.</summary>
public sealed class BatchTrackingState
{
    private IReadOnlyList<BatchRowDto> _batches = [];
    private bool _isLoading;
    private string? _errorMessage;
    private int _currentPage = 1;
    private int _totalCount;
    private int _totalPages;

    /// <summary>Gets the current page of batches.</summary>
    public IReadOnlyList<BatchRowDto> Batches => _batches;

    /// <summary>Gets a value indicating whether the page is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? ErrorMessage => _errorMessage;

    /// <summary>Gets the current page number.</summary>
    public int CurrentPage => _currentPage;

    /// <summary>Gets the total matching count.</summary>
    public int TotalCount => _totalCount;

    /// <summary>Gets the total page count.</summary>
    public int TotalPages => _totalPages;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the page as loading.</summary>
    public void BeginLoad() { _isLoading = true; _errorMessage = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded page of batches.</summary>
    public void SetBatches(PaginatedResponseDto<BatchRowDto> page)
    {
        _batches = page.Items;
        _currentPage = page.PageNumber;
        _totalCount = page.TotalCount;
        _totalPages = page.TotalPages;
        _isLoading = false;
        _errorMessage = null;
        NotifyStateChanged();
    }

    /// <summary>Records a load failure with a sanitized message.</summary>
    public void SetError(string sanitizedError) { _errorMessage = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}

/// <summary>Singleton state container for the Production Timeline page.</summary>
public sealed class ProductionTimelineState
{
    private IReadOnlyList<TimelineEventDto> _events = [];
    private bool _isLoading;
    private string? _errorMessage;

    /// <summary>Gets the plant-wide chronological events currently loaded.</summary>
    public IReadOnlyList<TimelineEventDto> Events => _events;

    /// <summary>Gets a value indicating whether the page is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? ErrorMessage => _errorMessage;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the page as loading.</summary>
    public void BeginLoad() { _isLoading = true; _errorMessage = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded page of chronological events.</summary>
    public void SetEvents(PaginatedResponseDto<TimelineEventDto> page)
    {
        _events = page.Items;
        _isLoading = false;
        _errorMessage = null;
        NotifyStateChanged();
    }

    /// <summary>Records a load failure with a sanitized message.</summary>
    public void SetError(string sanitizedError) { _errorMessage = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}

/// <summary>Singleton state container for the KPI Dashboard page.</summary>
public sealed class KpiDashboardState
{
    private IReadOnlyList<KpiTileDto> _kpis = [];
    private bool _isLoading;
    private string? _errorMessage;

    /// <summary>Gets the plant-wide KPI tiles currently loaded.</summary>
    public IReadOnlyList<KpiTileDto> Kpis => _kpis;

    /// <summary>Gets a value indicating whether the page is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? ErrorMessage => _errorMessage;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the page as loading.</summary>
    public void BeginLoad() { _isLoading = true; _errorMessage = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded page of KPI tiles.</summary>
    public void SetKpis(PaginatedResponseDto<KpiTileDto> page)
    {
        _kpis = page.Items;
        _isLoading = false;
        _errorMessage = null;
        NotifyStateChanged();
    }

    /// <summary>Records a load failure with a sanitized message.</summary>
    public void SetError(string sanitizedError) { _errorMessage = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}

/// <summary>Singleton state container for the Production Event History page.</summary>
public sealed class EventHistoryState
{
    private IReadOnlyList<EventRowDto> _events = [];
    private bool _isLoading;
    private string? _errorMessage;
    private int _currentPage = 1;
    private int _totalCount;
    private int _totalPages;

    /// <summary>Gets the current page of events.</summary>
    public IReadOnlyList<EventRowDto> Events => _events;

    /// <summary>Gets a value indicating whether the page is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? ErrorMessage => _errorMessage;

    /// <summary>Gets the current page number.</summary>
    public int CurrentPage => _currentPage;

    /// <summary>Gets the total matching count.</summary>
    public int TotalCount => _totalCount;

    /// <summary>Gets the total page count.</summary>
    public int TotalPages => _totalPages;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the page as loading.</summary>
    public void BeginLoad() { _isLoading = true; _errorMessage = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded page of events.</summary>
    public void SetEvents(PaginatedResponseDto<EventRowDto> page)
    {
        _events = page.Items;
        _currentPage = page.PageNumber;
        _totalCount = page.TotalCount;
        _totalPages = page.TotalPages;
        _isLoading = false;
        _errorMessage = null;
        NotifyStateChanged();
    }

    /// <summary>Records a load failure with a sanitized message.</summary>
    public void SetError(string sanitizedError) { _errorMessage = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
