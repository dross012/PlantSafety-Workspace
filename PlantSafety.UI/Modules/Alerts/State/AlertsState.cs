// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-06-17
// UPDATED: 2026-08-05 — List now holds AlertDto rows (matching the real
//   GET v1/alerts/history payload) instead of the fabricated
//   AlertListItemDto shape. Added busy/message/error state for the module's
//   real lifecycle write actions (Trigger/Dispatch/Escalate/Resolve/Close)
//   alongside the existing Acknowledge.
// FILE DESCRIPTION: Singleton state container for the Alerts module. Holds the
//   current paginated alert list, the selected alert, and loading/error state.
//   Components never call HttpClient directly — they dispatch to this state
//   via the AlertsApiClient, which is injected by components that trigger loads.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Models.Common;
using PlantSafety.UI.Modules.Alerts.Models;

namespace PlantSafety.UI.Modules.Alerts.State;

/// <summary>
/// Singleton state container for the Alerts module list view.
/// </summary>
public sealed class AlertsState
{
    private IReadOnlyList<AlertDto> _alerts = [];
    private AlertDto? _selectedAlert;
    private bool _isLoadingList;
    private bool _isLoadingDetail;
    private string? _listError;
    private string? _detailError;
    private int _currentPage = 1;
    private int _totalCount;
    private int _totalPages;

    private bool _isBusy;
    private string? _actionMessage;
    private string? _actionError;
    private bool _isCreating;
    private string? _createError;

    /// <summary>Gets the current page of alert list items. Never null; empty when not loaded.</summary>
    public IReadOnlyList<AlertDto> Alerts => _alerts;

    /// <summary>Gets the currently selected alert detail, or null when none is selected.</summary>
    public AlertDto? SelectedAlert => _selectedAlert;

    /// <summary>Gets a value indicating whether the list is loading.</summary>
    public bool IsLoadingList => _isLoadingList;

    /// <summary>Gets a value indicating whether a detail record is loading.</summary>
    public bool IsLoadingDetail => _isLoadingDetail;

    /// <summary>Gets a sanitized list load error, or null.</summary>
    public string? ListError => _listError;

    /// <summary>Gets a sanitized detail load error, or null.</summary>
    public string? DetailError => _detailError;

    /// <summary>Gets the current page number (one-based).</summary>
    public int CurrentPage => _currentPage;

    /// <summary>Gets the total matching alert count across all pages.</summary>
    public int TotalCount => _totalCount;

    /// <summary>Gets the total number of pages.</summary>
    public int TotalPages => _totalPages;

    /// <summary>Gets a value indicating whether a lifecycle action is in flight.</summary>
    public bool IsBusy => _isBusy;

    /// <summary>Gets the last lifecycle action's informational message, or null.</summary>
    public string? ActionMessage => _actionMessage;

    /// <summary>Gets the last lifecycle action's error message, or null.</summary>
    public string? ActionError => _actionError;

    /// <summary>Gets a value indicating whether a create (Trigger) operation is in flight.</summary>
    public bool IsCreating => _isCreating;

    /// <summary>Gets the last create operation's error message, or null.</summary>
    public string? CreateError => _createError;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the list as loading.</summary>
    public void BeginListLoad() { _isLoadingList = true; _listError = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded page of alerts.</summary>
    /// <param name="page">The paginated response from the API.</param>
    public void SetAlerts(PaginatedResponseDto<AlertDto> page)
    {
        _alerts = page.Items;
        _currentPage = page.PageNumber;
        _totalCount = page.TotalCount;
        _totalPages = page.TotalPages;
        _isLoadingList = false;
        _listError = null;
        NotifyStateChanged();
    }

    /// <summary>Records a list load failure with a sanitized message.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetListError(string sanitizedError) { _listError = sanitizedError; _isLoadingList = false; NotifyStateChanged(); }

    /// <summary>Marks the detail panel as loading.</summary>
    public void BeginDetailLoad() { _isLoadingDetail = true; _detailError = null; NotifyStateChanged(); }

    /// <summary>Applies the loaded alert detail record.</summary>
    /// <param name="alert">The full alert DTO from the API.</param>
    public void SetSelectedAlert(AlertDto alert) { _selectedAlert = alert; _isLoadingDetail = false; _detailError = null; NotifyStateChanged(); }

    /// <summary>Records a detail load failure.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetDetailError(string sanitizedError) { _detailError = sanitizedError; _isLoadingDetail = false; NotifyStateChanged(); }

    /// <summary>Clears the selected alert.</summary>
    public void ClearSelection() { _selectedAlert = null; NotifyStateChanged(); }

    /// <summary>Marks a lifecycle action as in flight.</summary>
    public void BeginAction() { _isBusy = true; _actionError = null; NotifyStateChanged(); }

    /// <summary>Records a successful lifecycle action's informational message.</summary>
    public void CompleteAction(string message) { _isBusy = false; _actionMessage = message; _actionError = null; NotifyStateChanged(); }

    /// <summary>Records a failed lifecycle action with a sanitized message.</summary>
    public void SetActionError(string sanitizedError) { _isBusy = false; _actionError = sanitizedError; NotifyStateChanged(); }

    /// <summary>Marks a create (Trigger) operation as in flight.</summary>
    public void BeginCreate() { _isCreating = true; _createError = null; NotifyStateChanged(); }

    /// <summary>Records a failed create operation with a sanitized message.</summary>
    public void SetCreateError(string sanitizedError) { _isCreating = false; _createError = sanitizedError; NotifyStateChanged(); }

    /// <summary>Resets create-form state.</summary>
    public void ResetCreate() { _isCreating = false; _createError = null; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
