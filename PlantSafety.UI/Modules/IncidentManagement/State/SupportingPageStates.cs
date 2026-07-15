// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state containers for the Incident SLA Monitor
//   (Volume ISM1) and Incident Timeline (Volume IT1) pages.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Modules.IncidentManagement.Models;

namespace PlantSafety.UI.Modules.IncidentManagement.State;

/// <summary>Singleton state container for the Incident SLA Monitor page (Volume ISM1).</summary>
public sealed class SlaMonitorState
{
    private IReadOnlyList<SlaIncidentDto> _incidents = [];
    private IReadOnlyList<SlaRiskDto> _risk = [];
    private SlaTrendDto? _trend;
    private bool _isLoading;
    private string? _errorMessage;
    private DateTimeOffset _lastRefreshUtc;

    /// <summary>Gets the SLA countdown grid rows.</summary>
    public IReadOnlyList<SlaIncidentDto> Incidents => _incidents;

    /// <summary>Gets the SLA risk panel rows (breached incidents sort first).</summary>
    public IReadOnlyList<SlaRiskDto> Risk => _risk;

    /// <summary>Gets the SLA trend summary, or null when not yet loaded.</summary>
    public SlaTrendDto? Trend => _trend;

    /// <summary>Gets a value indicating whether the monitor is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? ErrorMessage => _errorMessage;

    /// <summary>Gets the UTC timestamp of the last successful refresh.</summary>
    public DateTimeOffset LastRefreshUtc => _lastRefreshUtc;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the monitor as loading.</summary>
    public void BeginLoad() { _isLoading = true; _errorMessage = null; NotifyStateChanged(); }

    /// <summary>Applies a fully-refreshed SLA monitor snapshot. Breached incidents sort to the top of the risk panel.</summary>
    public void SetSnapshot(IReadOnlyList<SlaIncidentDto> incidents, IReadOnlyList<SlaRiskDto> risk, SlaTrendDto trend)
    {
        _incidents = [.. incidents.OrderBy(i => i.SlaRemaining)];
        _risk = [.. risk.OrderBy(r => r.SlaRemaining)];
        _trend = trend;
        _isLoading = false;
        _errorMessage = null;
        _lastRefreshUtc = DateTimeOffset.UtcNow;
        NotifyStateChanged();
    }

    /// <summary>Records a load failure with a sanitized message.</summary>
    public void SetError(string sanitizedError) { _errorMessage = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}

/// <summary>Singleton state container for the Incident Timeline page (Volume IT1).</summary>
public sealed class IncidentTimelinePageState
{
    private IReadOnlyList<IncidentTimelineEntryDto> _events = [];
    private string _search = string.Empty;
    private bool _isLoading;
    private string? _errorMessage;

    /// <summary>Gets the chronological timeline events.</summary>
    public IReadOnlyList<IncidentTimelineEntryDto> Events => _events;

    /// <summary>Gets the current search text filter.</summary>
    public string Search => _search;

    /// <summary>Gets a value indicating whether the timeline is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? ErrorMessage => _errorMessage;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the timeline as loading.</summary>
    public void BeginLoad() { _isLoading = true; _errorMessage = null; NotifyStateChanged(); }

    /// <summary>Applies loaded timeline events, reordered chronologically.</summary>
    public void SetEvents(IReadOnlyList<IncidentTimelineEntryDto> events)
    {
        _events = [.. events.OrderBy(e => e.TimestampUtc)];
        _isLoading = false;
        _errorMessage = null;
        NotifyStateChanged();
    }

    /// <summary>Applies a new client-side search filter (debounced by SearchBoxComponent).</summary>
    public void SetSearch(string search) { _search = search; NotifyStateChanged(); }

    /// <summary>Records a load failure with a sanitized message.</summary>
    public void SetError(string sanitizedError) { _errorMessage = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
