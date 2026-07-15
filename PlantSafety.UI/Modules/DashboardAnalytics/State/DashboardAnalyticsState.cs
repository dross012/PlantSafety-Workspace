// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Dashboard Analytics
//   page. Holds independent loading/error/data state for each of the four
//   data-backed tabs (Executive, KPI Snapshots, Trend, Read Model) plus the
//   shared date-range filter. Components never call HttpClient directly —
//   they dispatch to this state via DashboardAnalyticsApiClient.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Modules.DashboardAnalytics.Models;

namespace PlantSafety.UI.Modules.DashboardAnalytics.State;

/// <summary>The tabs presented on the Dashboard Analytics page.</summary>
public enum DashboardAnalyticsTab
{
    /// <summary>Executive summary metrics tab.</summary>
    Executive,

    /// <summary>KPI snapshots tab.</summary>
    KpiSnapshots,

    /// <summary>Trend analysis tab.</summary>
    Trend,

    /// <summary>Generic read-model widget rows tab.</summary>
    ReadModel
}

/// <summary>Singleton state container for the Dashboard Analytics page.</summary>
public sealed class DashboardAnalyticsState
{
    private DashboardAnalyticsTab _activeTab = DashboardAnalyticsTab.Executive;

    private DateOnly _filterStart = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-90));
    private DateOnly _filterEnd = DateOnly.FromDateTime(DateTime.UtcNow);

    private IReadOnlyList<ExecutiveMetricDto> _executive = [];
    private IReadOnlyList<KpiSnapshotDto> _kpiSnapshots = [];
    private IReadOnlyList<DashboardWidgetRowDto> _readModel = [];
    private IReadOnlyList<DashboardTrendPointDto> _trend = [];

    private bool _isLoadingExecutive;
    private bool _isLoadingKpiSnapshots;
    private bool _isLoadingReadModel;
    private bool _isLoadingTrend;

    private string? _executiveError;
    private string? _kpiSnapshotsError;
    private string? _readModelError;
    private string? _trendError;

    /// <summary>Gets the currently active tab.</summary>
    public DashboardAnalyticsTab ActiveTab => _activeTab;

    /// <summary>Gets the inclusive filter start date.</summary>
    public DateOnly FilterStart => _filterStart;

    /// <summary>Gets the inclusive filter end date.</summary>
    public DateOnly FilterEnd => _filterEnd;

    /// <summary>Gets the loaded Executive summary metrics.</summary>
    public IReadOnlyList<ExecutiveMetricDto> Executive => _executive;

    /// <summary>Gets the loaded KPI snapshots.</summary>
    public IReadOnlyList<KpiSnapshotDto> KpiSnapshots => _kpiSnapshots;

    /// <summary>Gets the loaded generic read-model widget rows.</summary>
    public IReadOnlyList<DashboardWidgetRowDto> ReadModel => _readModel;

    /// <summary>Gets the loaded trend data points.</summary>
    public IReadOnlyList<DashboardTrendPointDto> Trend => _trend;

    /// <summary>Gets a value indicating whether the Executive tab is loading.</summary>
    public bool IsLoadingExecutive => _isLoadingExecutive;

    /// <summary>Gets a value indicating whether the KPI Snapshots tab is loading.</summary>
    public bool IsLoadingKpiSnapshots => _isLoadingKpiSnapshots;

    /// <summary>Gets a value indicating whether the Read Model tab is loading.</summary>
    public bool IsLoadingReadModel => _isLoadingReadModel;

    /// <summary>Gets a value indicating whether the Trend tab is loading.</summary>
    public bool IsLoadingTrend => _isLoadingTrend;

    /// <summary>Gets a sanitized Executive load error, or null.</summary>
    public string? ExecutiveError => _executiveError;

    /// <summary>Gets a sanitized KPI Snapshots load error, or null.</summary>
    public string? KpiSnapshotsError => _kpiSnapshotsError;

    /// <summary>Gets a sanitized Read Model load error, or null.</summary>
    public string? ReadModelError => _readModelError;

    /// <summary>Gets a sanitized Trend load error, or null.</summary>
    public string? TrendError => _trendError;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Switches the active tab.</summary>
    public void SetActiveTab(DashboardAnalyticsTab tab) { _activeTab = tab; NotifyStateChanged(); }

    /// <summary>Applies a new date-range filter. Callers MUST reload the active tab afterward.</summary>
    public void SetFilter(DateOnly start, DateOnly end)
    {
        _filterStart = start;
        _filterEnd = end;
        _executive = [];
        _kpiSnapshots = [];
        _readModel = [];
        _trend = [];
        NotifyStateChanged();
    }

    /// <summary>Marks the Executive tab as loading.</summary>
    public void BeginExecutiveLoad() { _isLoadingExecutive = true; _executiveError = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded Executive metrics.</summary>
    public void SetExecutive(IReadOnlyList<ExecutiveMetricDto> items) { _executive = items; _isLoadingExecutive = false; _executiveError = null; NotifyStateChanged(); }

    /// <summary>Records an Executive load failure.</summary>
    public void SetExecutiveError(string sanitizedError) { _executiveError = sanitizedError; _isLoadingExecutive = false; NotifyStateChanged(); }

    /// <summary>Marks the KPI Snapshots tab as loading.</summary>
    public void BeginKpiSnapshotsLoad() { _isLoadingKpiSnapshots = true; _kpiSnapshotsError = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded KPI snapshots.</summary>
    public void SetKpiSnapshots(IReadOnlyList<KpiSnapshotDto> items) { _kpiSnapshots = items; _isLoadingKpiSnapshots = false; _kpiSnapshotsError = null; NotifyStateChanged(); }

    /// <summary>Records a KPI Snapshots load failure.</summary>
    public void SetKpiSnapshotsError(string sanitizedError) { _kpiSnapshotsError = sanitizedError; _isLoadingKpiSnapshots = false; NotifyStateChanged(); }

    /// <summary>Marks the Read Model tab as loading.</summary>
    public void BeginReadModelLoad() { _isLoadingReadModel = true; _readModelError = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded read-model widget rows.</summary>
    public void SetReadModel(IReadOnlyList<DashboardWidgetRowDto> items) { _readModel = items; _isLoadingReadModel = false; _readModelError = null; NotifyStateChanged(); }

    /// <summary>Records a Read Model load failure.</summary>
    public void SetReadModelError(string sanitizedError) { _readModelError = sanitizedError; _isLoadingReadModel = false; NotifyStateChanged(); }

    /// <summary>Marks the Trend tab as loading.</summary>
    public void BeginTrendLoad() { _isLoadingTrend = true; _trendError = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded trend data points.</summary>
    public void SetTrend(IReadOnlyList<DashboardTrendPointDto> items) { _trend = items; _isLoadingTrend = false; _trendError = null; NotifyStateChanged(); }

    /// <summary>Records a Trend load failure.</summary>
    public void SetTrendError(string sanitizedError) { _trendError = sanitizedError; _isLoadingTrend = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
