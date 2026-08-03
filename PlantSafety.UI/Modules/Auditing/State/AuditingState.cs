// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Auditing module. Holds
//   the currently loaded audit list (for the active date range) and the
//   selected audit detail. Components never call HttpClient directly — they
//   dispatch to this state via AuditingApiClient, which is injected by the
//   components that trigger loads.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Modules.Auditing.Models;

namespace PlantSafety.UI.Modules.Auditing.State;

/// <summary>Singleton state container for the Auditing module.</summary>
public sealed class AuditingState
{
    private IReadOnlyList<AuditItemDto> _items = [];
    private AuditItemDto? _selectedItem;
    private bool _isLoadingList;
    private bool _isLoadingDetail;
    private string? _listError;
    private string? _detailError;

    /// <summary>Gets the currently loaded audit records for the active date range. Never null.</summary>
    public IReadOnlyList<AuditItemDto> Items => _items;

    /// <summary>Gets the currently selected audit detail, or null when none is selected.</summary>
    public AuditItemDto? SelectedItem => _selectedItem;

    /// <summary>Gets a value indicating whether the list is loading.</summary>
    public bool IsLoadingList => _isLoadingList;

    /// <summary>Gets a value indicating whether a detail record is loading.</summary>
    public bool IsLoadingDetail => _isLoadingDetail;

    /// <summary>Gets a sanitized list load error, or null.</summary>
    public string? ListError => _listError;

    /// <summary>Gets a sanitized detail load error, or null.</summary>
    public string? DetailError => _detailError;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the list as loading.</summary>
    public void BeginListLoad() { _isLoadingList = true; _listError = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded set of audit records.</summary>
    /// <param name="items">The items returned by the API for the requested date range.</param>
    public void SetItems(IReadOnlyList<AuditItemDto> items)
    {
        _items = items;
        _isLoadingList = false;
        _listError = null;
        NotifyStateChanged();
    }

    /// <summary>Records a list load failure with a sanitized message.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetListError(string sanitizedError) { _listError = sanitizedError; _isLoadingList = false; NotifyStateChanged(); }

    /// <summary>Marks the detail panel as loading.</summary>
    public void BeginDetailLoad() { _isLoadingDetail = true; _detailError = null; NotifyStateChanged(); }

    /// <summary>Applies the loaded audit detail record.</summary>
    /// <param name="item">The full audit record.</param>
    public void SetSelectedItem(AuditItemDto item) { _selectedItem = item; _isLoadingDetail = false; _detailError = null; NotifyStateChanged(); }

    /// <summary>Records a detail load failure.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetDetailError(string sanitizedError) { _detailError = sanitizedError; _isLoadingDetail = false; NotifyStateChanged(); }

    /// <summary>Clears the selected audit.</summary>
    public void ClearSelection() { _selectedItem = null; NotifyStateChanged(); }

    // ============================================================
    // Phase 5 §6.1 — AuditTimeline
    // ============================================================

    private AuditTimelineDto? _timeline;
    private bool _isLoadingTimeline;
    private string? _timelineError;

    /// <summary>Gets the currently loaded AuditTimeline, or null when none is loaded.</summary>
    public AuditTimelineDto? Timeline => _timeline;

    /// <summary>Gets a value indicating whether the timeline is loading.</summary>
    public bool IsLoadingTimeline => _isLoadingTimeline;

    /// <summary>Gets a sanitized timeline load error, or null.</summary>
    public string? TimelineError => _timelineError;

    /// <summary>Marks the timeline as loading.</summary>
    public void BeginTimelineLoad() { _isLoadingTimeline = true; _timelineError = null; NotifyStateChanged(); }

    /// <summary>Applies the freshly loaded timeline.</summary>
    public void SetTimeline(AuditTimelineDto timeline) { _timeline = timeline; _isLoadingTimeline = false; _timelineError = null; NotifyStateChanged(); }

    /// <summary>Records a timeline load failure.</summary>
    public void SetTimelineError(string sanitizedError) { _timelineError = sanitizedError; _isLoadingTimeline = false; NotifyStateChanged(); }

    // ============================================================
    // Phase 5 §6.2 — AuditReport
    // ============================================================

    private IReadOnlyList<AuditReportDto> _reportHistory = [];
    private AuditReportDto? _currentReport;
    private bool _isLoadingReportHistory;
    private bool _isGeneratingReport;
    private bool _isDownloadingReport;
    private string? _reportError;

    /// <summary>Gets the AuditReport generation history for the currently selected target entity.</summary>
    public IReadOnlyList<AuditReportDto> ReportHistory => _reportHistory;

    /// <summary>Gets the most recently generated/viewed AuditReport, or null.</summary>
    public AuditReportDto? CurrentReport => _currentReport;

    /// <summary>Gets a value indicating whether the report history is loading.</summary>
    public bool IsLoadingReportHistory => _isLoadingReportHistory;

    /// <summary>Gets a value indicating whether a report generation is in progress.</summary>
    public bool IsGeneratingReport => _isGeneratingReport;

    /// <summary>Gets a value indicating whether a report content download is in progress.</summary>
    public bool IsDownloadingReport => _isDownloadingReport;

    /// <summary>Gets a sanitized report action error, or null.</summary>
    public string? ReportError => _reportError;

    /// <summary>Marks the report history as loading.</summary>
    public void BeginReportHistoryLoad() { _isLoadingReportHistory = true; _reportError = null; NotifyStateChanged(); }

    /// <summary>Applies the freshly loaded report history.</summary>
    public void SetReportHistory(IReadOnlyList<AuditReportDto> reports) { _reportHistory = reports; _isLoadingReportHistory = false; _reportError = null; NotifyStateChanged(); }

    /// <summary>Records a report history load failure.</summary>
    public void SetReportHistoryError(string sanitizedError) { _reportHistory = []; _isLoadingReportHistory = false; _reportError = sanitizedError; NotifyStateChanged(); }

    /// <summary>Marks a report generation as in progress.</summary>
    public void BeginGenerateReport() { _isGeneratingReport = true; _reportError = null; NotifyStateChanged(); }

    /// <summary>Applies the freshly generated report and prepends it to the history.</summary>
    public void SetGeneratedReport(AuditReportDto report)
    {
        _currentReport = report;
        _reportHistory = new List<AuditReportDto> { report }.Concat(_reportHistory).ToList();
        _isGeneratingReport = false;
        _reportError = null;
        NotifyStateChanged();
    }

    /// <summary>Records a report generation failure.</summary>
    public void SetGenerateReportError(string sanitizedError) { _isGeneratingReport = false; _reportError = sanitizedError; NotifyStateChanged(); }

    /// <summary>Marks a report content download as in progress.</summary>
    public void BeginDownloadReport() { _isDownloadingReport = true; _reportError = null; NotifyStateChanged(); }

    /// <summary>Completes a report content download.</summary>
    public void CompleteDownloadReport() { _isDownloadingReport = false; _reportError = null; NotifyStateChanged(); }

    /// <summary>Records a report content download failure.</summary>
    public void SetDownloadReportError(string sanitizedError) { _isDownloadingReport = false; _reportError = sanitizedError; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
