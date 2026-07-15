// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Scheduling Analytics
//   page. Holds loading/error/data state for the single Schedules section
//   plus the shared date-range filter. Components never call HttpClient
//   directly — they dispatch to this state via SchedulingAnalyticsApiClient.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Modules.SchedulingAnalytics.Models;

namespace PlantSafety.UI.Modules.SchedulingAnalytics.State;

/// <summary>The tabs presented on the Scheduling Analytics page.</summary>
public enum SchedulingAnalyticsTab
{
    /// <summary>Overview summary tab.</summary>
    Overview,

    /// <summary>Full schedules table tab.</summary>
    Schedules,

    /// <summary>Upcoming runs tab.</summary>
    Upcoming
}

/// <summary>Singleton state container for the Scheduling Analytics page.</summary>
public sealed class SchedulingAnalyticsState
{
    private SchedulingAnalyticsTab _activeTab = SchedulingAnalyticsTab.Overview;

    private DateOnly _filterStart = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-90));
    private DateOnly _filterEnd = DateOnly.FromDateTime(DateTime.UtcNow);

    private IReadOnlyList<ScheduleEntryDto> _schedules = [];
    private bool _isLoading;
    private string? _error;

    /// <summary>Gets the currently active tab.</summary>
    public SchedulingAnalyticsTab ActiveTab => _activeTab;

    /// <summary>Gets the inclusive filter start date.</summary>
    public DateOnly FilterStart => _filterStart;

    /// <summary>Gets the inclusive filter end date.</summary>
    public DateOnly FilterEnd => _filterEnd;

    /// <summary>Gets the loaded schedule entries.</summary>
    public IReadOnlyList<ScheduleEntryDto> Schedules => _schedules;

    /// <summary>Gets a value indicating whether data is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? Error => _error;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Switches the active tab.</summary>
    public void SetActiveTab(SchedulingAnalyticsTab tab) { _activeTab = tab; NotifyStateChanged(); }

    /// <summary>Applies a new date-range filter. Callers MUST reload afterward.</summary>
    public void SetFilter(DateOnly start, DateOnly end)
    {
        _filterStart = start;
        _filterEnd = end;
        _schedules = [];
        NotifyStateChanged();
    }

    /// <summary>Marks the schedules as loading.</summary>
    public void BeginLoad() { _isLoading = true; _error = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded schedule entries.</summary>
    public void SetSchedules(IReadOnlyList<ScheduleEntryDto> items) { _schedules = items; _isLoading = false; _error = null; NotifyStateChanged(); }

    /// <summary>Records a load failure.</summary>
    public void SetError(string sanitizedError) { _error = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
