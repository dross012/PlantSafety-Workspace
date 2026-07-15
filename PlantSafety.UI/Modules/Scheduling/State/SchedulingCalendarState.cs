// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Scheduling Calendar
//   page (Volume S1 §3). Holds the current 7-day week grid. 10-minute
//   auto-refresh interval per Volume S1 performance budget table.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Modules.Scheduling.Models;

namespace PlantSafety.UI.Modules.Scheduling.State;

/// <summary>Singleton state container for the Scheduling Calendar page.</summary>
public sealed class SchedulingCalendarState
{
    private CalendarWeekDto? _week;
    private bool _isLoading;
    private string? _errorMessage;
    private DateTimeOffset _lastRefreshUtc;

    /// <summary>Gets the currently displayed week, or null when not yet loaded.</summary>
    public CalendarWeekDto? Week => _week;

    /// <summary>Gets a value indicating whether the calendar is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? ErrorMessage => _errorMessage;

    /// <summary>Gets the UTC timestamp of the last successful refresh.</summary>
    public DateTimeOffset LastRefreshUtc => _lastRefreshUtc;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the calendar as loading.</summary>
    public void BeginLoad() { _isLoading = true; _errorMessage = null; NotifyStateChanged(); }

    /// <summary>Applies the loaded week grid.</summary>
    public void SetWeek(CalendarWeekDto week)
    {
        _week = week;
        _isLoading = false;
        _errorMessage = null;
        _lastRefreshUtc = DateTimeOffset.UtcNow;
        NotifyStateChanged();
    }

    /// <summary>Records a load failure with a sanitized message.</summary>
    public void SetError(string sanitizedError) { _errorMessage = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
