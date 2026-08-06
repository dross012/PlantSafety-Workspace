// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Incident Detail page
//   (Volume ID1). Holds the incident header/summary and its timeline
//   (embedded panel per ID1 Tier 2 right column).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Modules.IncidentManagement.Models;

namespace PlantSafety.UI.Modules.IncidentManagement.State;

/// <summary>Singleton state container for the Incident Detail page.</summary>
public sealed class IncidentDetailState
{
    private IncidentDetailDto? _incident;
    private IReadOnlyList<IncidentTimelineEntryDto> _timeline = [];
    private bool _isLoading;
    private bool _isLoadingTimeline;
    private string? _errorMessage;
    private bool _isBusy;
    private string? _actionMessage;
    private string? _actionError;

    /// <summary>Gets the loaded incident, or null when not yet loaded.</summary>
    public IncidentDetailDto? Incident => _incident;

    /// <summary>Gets the embedded timeline panel entries.</summary>
    public IReadOnlyList<IncidentTimelineEntryDto> Timeline => _timeline;

    /// <summary>Gets a value indicating whether the incident header is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a value indicating whether the timeline panel is loading.</summary>
    public bool IsLoadingTimeline => _isLoadingTimeline;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? ErrorMessage => _errorMessage;

    /// <summary>Gets a value indicating whether a lifecycle action (assign/escalate/note/resolve/edit/delete) is in flight.</summary>
    public bool IsBusy => _isBusy;

    /// <summary>Gets the most recent successful action's confirmation message, or null.</summary>
    public string? ActionMessage => _actionMessage;

    /// <summary>Gets the most recent action failure's sanitized message, or null.</summary>
    public string? ActionError => _actionError;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the incident header as loading.</summary>
    public void BeginLoad() { _isLoading = true; _errorMessage = null; NotifyStateChanged(); }

    /// <summary>Applies the loaded incident header.</summary>
    public void SetIncident(IncidentDetailDto incident) { _incident = incident; _isLoading = false; _errorMessage = null; NotifyStateChanged(); }

    /// <summary>Marks the timeline panel as loading.</summary>
    public void BeginTimelineLoad() { _isLoadingTimeline = true; NotifyStateChanged(); }

    /// <summary>Applies loaded timeline entries, reordered chronologically.</summary>
    public void SetTimeline(IReadOnlyList<IncidentTimelineEntryDto> timeline)
    {
        _timeline = [.. timeline.OrderBy(e => e.TimestampUtc)];
        _isLoadingTimeline = false;
        NotifyStateChanged();
    }

    /// <summary>Records a load failure with a sanitized message.</summary>
    public void SetError(string sanitizedError) { _errorMessage = sanitizedError; _isLoading = false; _isLoadingTimeline = false; NotifyStateChanged(); }

    /// <summary>Marks a lifecycle action as in flight.</summary>
    public void BeginAction() { _isBusy = true; _actionError = null; _actionMessage = null; NotifyStateChanged(); }

    /// <summary>Records a lifecycle action success with a confirmation message.</summary>
    public void CompleteAction(string message) { _isBusy = false; _actionMessage = message; _actionError = null; NotifyStateChanged(); }

    /// <summary>Records a lifecycle action failure with a sanitized message.</summary>
    public void SetActionError(string sanitizedError) { _isBusy = false; _actionError = sanitizedError; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
