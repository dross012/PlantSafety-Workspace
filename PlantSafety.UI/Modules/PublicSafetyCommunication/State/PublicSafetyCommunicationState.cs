// ============================================================
// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Singleton state container for the PublicSafetyCommunication module. Holds
//   data for the Agency Notification Status Board dashboard (agency directory + notification
//   dispatch status), Broadcast History dashboard (active broadcasts), and After-Action
//   Communication Timeline dashboard (looked up by incident ID), plus loading/error state.
//   Components dispatch to this state via PublicSafetyCommunicationApiClient.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Modules.PublicSafetyCommunication.Models;

namespace PlantSafety.UI.Modules.PublicSafetyCommunication.State;

/// <summary>Singleton state container for the PublicSafetyCommunication module dashboards.</summary>
public sealed class PublicSafetyCommunicationState
{
    private IReadOnlyList<PublicSafetyAgencyDto> _agencies = [];
    private IReadOnlyList<IncidentNotificationDto> _notifications = [];
    private bool _isLoadingStatusBoard;
    private string? _statusBoardError;

    private IReadOnlyList<PublicAlertBroadcastDto> _activeBroadcasts = [];
    private bool _isLoadingBroadcasts;
    private string? _broadcastsError;

    private Guid? _timelineIncidentId;
    private IReadOnlyList<CommunicationLogEntryDto> _communicationLog = [];
    private bool _isLoadingTimeline;
    private string? _timelineError;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Gets the public-safety agency directory.</summary>
    public IReadOnlyList<PublicSafetyAgencyDto> Agencies => _agencies;

    /// <summary>Gets the incident notification dispatch records.</summary>
    public IReadOnlyList<IncidentNotificationDto> Notifications => _notifications;

    /// <summary>Gets a value indicating whether the Agency Notification Status Board is loading.</summary>
    public bool IsLoadingStatusBoard => _isLoadingStatusBoard;

    /// <summary>Gets a sanitized Agency Notification Status Board load error, or null.</summary>
    public string? StatusBoardError => _statusBoardError;

    /// <summary>Gets the currently active public alert broadcasts.</summary>
    public IReadOnlyList<PublicAlertBroadcastDto> ActiveBroadcasts => _activeBroadcasts;

    /// <summary>Gets a value indicating whether the Broadcast History dashboard is loading.</summary>
    public bool IsLoadingBroadcasts => _isLoadingBroadcasts;

    /// <summary>Gets a sanitized Broadcast History load error, or null.</summary>
    public string? BroadcastsError => _broadcastsError;

    /// <summary>Gets the incident currently selected for the After-Action Communication Timeline, if any.</summary>
    public Guid? TimelineIncidentId => _timelineIncidentId;

    /// <summary>Gets the communication log entries for the selected incident.</summary>
    public IReadOnlyList<CommunicationLogEntryDto> CommunicationLog => _communicationLog;

    /// <summary>Gets a value indicating whether the After-Action Communication Timeline is loading.</summary>
    public bool IsLoadingTimeline => _isLoadingTimeline;

    /// <summary>Gets a sanitized After-Action Communication Timeline load error, or null.</summary>
    public string? TimelineError => _timelineError;

    /// <summary>Marks the Agency Notification Status Board as loading.</summary>
    public void BeginStatusBoardLoad() { _isLoadingStatusBoard = true; _statusBoardError = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded agency-directory and notification-status data.</summary>
    /// <param name="agencies">The public-safety agency directory.</param>
    /// <param name="notifications">The incident notification dispatch records.</param>
    public void SetStatusBoardData(IReadOnlyList<PublicSafetyAgencyDto> agencies, IReadOnlyList<IncidentNotificationDto> notifications)
    {
        _agencies = agencies;
        _notifications = notifications;
        _isLoadingStatusBoard = false;
        _statusBoardError = null;
        NotifyStateChanged();
    }

    /// <summary>Records an Agency Notification Status Board load failure with a sanitized message.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetStatusBoardError(string sanitizedError) { _statusBoardError = sanitizedError; _isLoadingStatusBoard = false; NotifyStateChanged(); }

    /// <summary>Marks the Broadcast History dashboard as loading.</summary>
    public void BeginBroadcastsLoad() { _isLoadingBroadcasts = true; _broadcastsError = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded active-broadcasts set.</summary>
    /// <param name="items">The full set of active broadcast records.</param>
    public void SetActiveBroadcasts(IReadOnlyList<PublicAlertBroadcastDto> items) { _activeBroadcasts = items; _isLoadingBroadcasts = false; _broadcastsError = null; NotifyStateChanged(); }

    /// <summary>Records a Broadcast History load failure with a sanitized message.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetBroadcastsError(string sanitizedError) { _broadcastsError = sanitizedError; _isLoadingBroadcasts = false; NotifyStateChanged(); }

    /// <summary>Marks the After-Action Communication Timeline as loading for the given incident.</summary>
    /// <param name="incidentId">The incident identifier being summarized.</param>
    public void BeginTimelineLoad(Guid incidentId) { _timelineIncidentId = incidentId; _isLoadingTimeline = true; _timelineError = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded communication log for the selected incident.</summary>
    /// <param name="items">The full communication log for the incident, ordered chronologically.</param>
    public void SetCommunicationLog(IReadOnlyList<CommunicationLogEntryDto> items) { _communicationLog = items; _isLoadingTimeline = false; _timelineError = null; NotifyStateChanged(); }

    /// <summary>Records an After-Action Communication Timeline load failure with a sanitized message.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetTimelineError(string sanitizedError) { _timelineError = sanitizedError; _isLoadingTimeline = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
