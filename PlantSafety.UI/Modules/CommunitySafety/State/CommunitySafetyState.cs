// ============================================================
// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Singleton state container for the CommunitySafety module. Holds data for the
//   Community Concern Tracker dashboard (open concerns), Notification History &amp; Reach dashboard
//   (notifications over a period), and Impact Assessment Summary by incident dashboard (looked up
//   by incident ID), plus loading/error state. Components dispatch to this state via
//   CommunitySafetyApiClient.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Modules.CommunitySafety.Models;

namespace PlantSafety.UI.Modules.CommunitySafety.State;

/// <summary>Singleton state container for the CommunitySafety module dashboards.</summary>
public sealed class CommunitySafetyState
{
    private IReadOnlyList<CommunityConcernDto> _openConcerns = [];
    private bool _isLoadingConcerns;
    private string? _concernsError;

    private IReadOnlyList<CommunityNotificationDto> _notifications = [];
    private bool _isLoadingNotifications;
    private string? _notificationsError;

    private Guid? _impactIncidentId;
    private IReadOnlyList<CommunityImpactAssessmentDto> _impactAssessments = [];
    private bool _isLoadingImpact;
    private string? _impactError;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Gets the currently open community concerns.</summary>
    public IReadOnlyList<CommunityConcernDto> OpenConcerns => _openConcerns;

    /// <summary>Gets a value indicating whether the Community Concern Tracker is loading.</summary>
    public bool IsLoadingConcerns => _isLoadingConcerns;

    /// <summary>Gets a sanitized Community Concern Tracker load error, or null.</summary>
    public string? ConcernsError => _concernsError;

    /// <summary>Gets the notification history for the selected period.</summary>
    public IReadOnlyList<CommunityNotificationDto> Notifications => _notifications;

    /// <summary>Gets a value indicating whether the Notification History &amp; Reach dashboard is loading.</summary>
    public bool IsLoadingNotifications => _isLoadingNotifications;

    /// <summary>Gets a sanitized Notification History &amp; Reach load error, or null.</summary>
    public string? NotificationsError => _notificationsError;

    /// <summary>Gets the incident currently selected for the Impact Assessment Summary, if any.</summary>
    public Guid? ImpactIncidentId => _impactIncidentId;

    /// <summary>Gets the impact assessments for the selected incident.</summary>
    public IReadOnlyList<CommunityImpactAssessmentDto> ImpactAssessments => _impactAssessments;

    /// <summary>Gets a value indicating whether the Impact Assessment Summary is loading.</summary>
    public bool IsLoadingImpact => _isLoadingImpact;

    /// <summary>Gets a sanitized Impact Assessment Summary load error, or null.</summary>
    public string? ImpactError => _impactError;

    /// <summary>Marks the Community Concern Tracker as loading.</summary>
    public void BeginConcernsLoad() { _isLoadingConcerns = true; _concernsError = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded open-concerns set.</summary>
    /// <param name="items">The full set of open community concern records.</param>
    public void SetOpenConcerns(IReadOnlyList<CommunityConcernDto> items) { _openConcerns = items; _isLoadingConcerns = false; _concernsError = null; NotifyStateChanged(); }

    /// <summary>Records a Community Concern Tracker load failure with a sanitized message.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetConcernsError(string sanitizedError) { _concernsError = sanitizedError; _isLoadingConcerns = false; NotifyStateChanged(); }

    /// <summary>Marks the Notification History &amp; Reach dashboard as loading.</summary>
    public void BeginNotificationsLoad() { _isLoadingNotifications = true; _notificationsError = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded notification-history set.</summary>
    /// <param name="items">The full set of community notification records.</param>
    public void SetNotifications(IReadOnlyList<CommunityNotificationDto> items) { _notifications = items; _isLoadingNotifications = false; _notificationsError = null; NotifyStateChanged(); }

    /// <summary>Records a Notification History &amp; Reach load failure with a sanitized message.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetNotificationsError(string sanitizedError) { _notificationsError = sanitizedError; _isLoadingNotifications = false; NotifyStateChanged(); }

    /// <summary>Marks the Impact Assessment Summary dashboard as loading for the given incident.</summary>
    /// <param name="incidentId">The SafetyEvents incident identifier being summarized.</param>
    public void BeginImpactLoad(Guid incidentId) { _impactIncidentId = incidentId; _isLoadingImpact = true; _impactError = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded impact-assessments set for the selected incident.</summary>
    /// <param name="items">The full set of impact assessment records for the incident.</param>
    public void SetImpactAssessments(IReadOnlyList<CommunityImpactAssessmentDto> items) { _impactAssessments = items; _isLoadingImpact = false; _impactError = null; NotifyStateChanged(); }

    /// <summary>Records an Impact Assessment Summary load failure with a sanitized message.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetImpactError(string sanitizedError) { _impactError = sanitizedError; _isLoadingImpact = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
