// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Scheduling Dashboard
//   page (Volume S1 §1). Holds the Job Summary, Resource Summary, and
//   Upcoming Jobs page. Immutable snapshot replaced wholesale on each load.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Models.Common;
using PlantSafety.UI.Modules.Scheduling.Models;

namespace PlantSafety.UI.Modules.Scheduling.State;

/// <summary>Singleton state container for the Scheduling Dashboard.</summary>
public sealed class SchedulingDashboardState
{
    private JobSummaryDto? _jobSummary;
    private ResourceSummaryDto? _resourceSummary;
    private IReadOnlyList<UpcomingJobDto> _upcomingJobs = [];
    private bool _isLoading;
    private string? _errorMessage;
    private int _currentPage = 1;
    private int _totalCount;
    private int _totalPages;
    private string? _statusFilter;
    private DateTimeOffset _lastRefreshUtc;

    /// <summary>Gets the Job Summary card data, or null when not yet loaded.</summary>
    public JobSummaryDto? JobSummary => _jobSummary;

    /// <summary>Gets the Resource Summary card data, or null when not yet loaded.</summary>
    public ResourceSummaryDto? ResourceSummary => _resourceSummary;

    /// <summary>Gets the current page of upcoming jobs. Never null; empty when not loaded.</summary>
    public IReadOnlyList<UpcomingJobDto> UpcomingJobs => _upcomingJobs;

    /// <summary>Gets a value indicating whether the dashboard is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? ErrorMessage => _errorMessage;

    /// <summary>Gets the current page number (one-based).</summary>
    public int CurrentPage => _currentPage;

    /// <summary>Gets the total matching job count across all pages.</summary>
    public int TotalCount => _totalCount;

    /// <summary>Gets the total number of pages.</summary>
    public int TotalPages => _totalPages;

    /// <summary>Gets the currently applied status filter, or null for all statuses.</summary>
    public string? StatusFilter => _statusFilter;

    /// <summary>Gets the UTC timestamp of the last successful refresh.</summary>
    public DateTimeOffset LastRefreshUtc => _lastRefreshUtc;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the dashboard as loading.</summary>
    public void BeginLoad() { _isLoading = true; _errorMessage = null; NotifyStateChanged(); }

    /// <summary>Applies the loaded summary cards.</summary>
    public void SetSummaries(JobSummaryDto jobSummary, ResourceSummaryDto resourceSummary)
    {
        _jobSummary = jobSummary;
        _resourceSummary = resourceSummary;
        NotifyStateChanged();
    }

    /// <summary>Applies a freshly loaded page of upcoming jobs.</summary>
    public void SetUpcomingJobs(PaginatedResponseDto<UpcomingJobDto> page)
    {
        _upcomingJobs = page.Items;
        _currentPage = page.PageNumber;
        _totalCount = page.TotalCount;
        _totalPages = page.TotalPages;
        _isLoading = false;
        _errorMessage = null;
        _lastRefreshUtc = DateTimeOffset.UtcNow;
        NotifyStateChanged();
    }

    /// <summary>Applies a new status filter (Reset Filters passes null).</summary>
    public void SetStatusFilter(string? statusFilter) { _statusFilter = statusFilter; NotifyStateChanged(); }

    /// <summary>Records a load failure with a sanitized message.</summary>
    public void SetError(string sanitizedError) { _errorMessage = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
