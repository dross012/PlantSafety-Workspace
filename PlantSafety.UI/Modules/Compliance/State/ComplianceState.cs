// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Compliance dashboard
//   module. Holds the three tab row snapshots (Requirements/Obligations/
//   Tracking), per-tab client-side pagination (the backing read-model
//   endpoints return a full date-range result set with no server-side
//   paging), and the selected detail record. Components never call
//   HttpClient directly — they dispatch to this state via ComplianceApiClient,
//   which is injected by the pages that trigger loads. Exposes
//   LoadAsync/RefreshAsync per the standard module state container contract.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;

using PlantSafety.UI.Modules.Compliance.Mappings;
using PlantSafety.UI.Modules.Compliance.Models;

namespace PlantSafety.UI.Modules.Compliance.State;

/// <summary>Singleton state container for the Compliance dashboard module.</summary>
public sealed class ComplianceState
{
    private const int PageSize = 10;

    private IReadOnlyList<ComplianceRowViewModel> _requirements = [];
    private IReadOnlyList<ComplianceRowViewModel> _obligations = [];
    private IReadOnlyList<ComplianceRowViewModel> _tracking = [];

    private readonly Dictionary<(ComplianceCategory, Guid), ComplianceDetailViewModel> _detailCache = [];

    private int _requirementsPage = 1;
    private int _obligationsPage = 1;
    private int _trackingPage = 1;

    private bool _isLoadingDashboard;
    private bool _isLoadingDetail;
    private string? _dashboardError;
    private string? _detailError;
    private DateTime _lastRefreshUtc;

    /// <summary>Gets all loaded Regulatory Requirement rows (unpaginated).</summary>
    public IReadOnlyList<ComplianceRowViewModel> Requirements => _requirements;

    /// <summary>Gets all loaded Obligation rows (unpaginated).</summary>
    public IReadOnlyList<ComplianceRowViewModel> Obligations => _obligations;

    /// <summary>Gets all loaded Tracking rows (unpaginated).</summary>
    public IReadOnlyList<ComplianceRowViewModel> Tracking => _tracking;

    /// <summary>Gets the current page of Requirement rows.</summary>
    public IReadOnlyList<ComplianceRowViewModel> RequirementsPageItems => Page(_requirements, _requirementsPage);

    /// <summary>Gets the current page of Obligation rows.</summary>
    public IReadOnlyList<ComplianceRowViewModel> ObligationsPageItems => Page(_obligations, _obligationsPage);

    /// <summary>Gets the current page of Tracking rows.</summary>
    public IReadOnlyList<ComplianceRowViewModel> TrackingPageItems => Page(_tracking, _trackingPage);

    /// <summary>Gets the current 1-based Requirements page number.</summary>
    public int RequirementsPage => _requirementsPage;

    /// <summary>Gets the current 1-based Obligations page number.</summary>
    public int ObligationsPage => _obligationsPage;

    /// <summary>Gets the current 1-based Tracking page number.</summary>
    public int TrackingPage => _trackingPage;

    /// <summary>Gets the total page count for Requirements.</summary>
    public int RequirementsTotalPages => TotalPages(_requirements);

    /// <summary>Gets the total page count for Obligations.</summary>
    public int ObligationsTotalPages => TotalPages(_obligations);

    /// <summary>Gets the total page count for Tracking.</summary>
    public int TrackingTotalPages => TotalPages(_tracking);

    /// <summary>Gets the count of Obligations currently Overdue — the primary Compliance KPI.</summary>
    public int OverdueObligationCount => _obligations.Count(r => r.StatusIndicatorState == "Critical");

    /// <summary>Gets the count of Tracking items currently At Risk or Overdue.</summary>
    public int AtRiskTrackingCount => _tracking.Count(r => r.StatusIndicatorState is "Critical" or "Warning");

    /// <summary>Gets the count of Requirements currently Under Review.</summary>
    public int UnderReviewRequirementCount => _requirements.Count(r => r.StatusIndicatorState == "Warning");

    /// <summary>Gets the currently selected detail record, or null when none is selected.</summary>
    public ComplianceDetailViewModel? SelectedItem { get; private set; }

    /// <summary>Gets a value indicating whether the dashboard is loading.</summary>
    public bool IsLoadingDashboard => _isLoadingDashboard;

    /// <summary>Gets a value indicating whether a detail record is loading.</summary>
    public bool IsLoadingDetail => _isLoadingDetail;

    /// <summary>Gets a sanitized dashboard load error, or null.</summary>
    public string? DashboardError => _dashboardError;

    /// <summary>Gets a sanitized detail load error, or null.</summary>
    public string? DetailError => _detailError;

    /// <summary>Gets the UTC timestamp of the last successful dashboard refresh.</summary>
    public DateTime LastRefreshUtc => _lastRefreshUtc;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the dashboard as loading.</summary>
    public void BeginDashboardLoad() { _isLoadingDashboard = true; _dashboardError = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded dashboard data for all three tabs and refreshes the detail cache.</summary>
    public void SetDashboardData(
        IReadOnlyList<RequirementItemDto> requirements,
        IReadOnlyList<ObligationItemDto> obligations,
        IReadOnlyList<TrackingItemDto> tracking)
    {
        _requirements = requirements.Select(Mappings.ComplianceMappingService.ToRowViewModel).ToList();
        _obligations = obligations.Select(Mappings.ComplianceMappingService.ToRowViewModel).ToList();
        _tracking = tracking.Select(Mappings.ComplianceMappingService.ToRowViewModel).ToList();

        _detailCache.Clear();
        foreach (var r in requirements) _detailCache[(ComplianceCategory.Requirement, r.RequirementId)] = ComplianceMappingService.ToDetailViewModel(r);
        foreach (var o in obligations) _detailCache[(ComplianceCategory.Obligation, o.ObligationId)] = ComplianceMappingService.ToDetailViewModel(o);
        foreach (var t in tracking) _detailCache[(ComplianceCategory.Tracking, t.TrackingItemId)] = ComplianceMappingService.ToDetailViewModel(t);

        _requirementsPage = 1;
        _obligationsPage = 1;
        _trackingPage = 1;
        _isLoadingDashboard = false;
        _dashboardError = null;
        _lastRefreshUtc = DateTime.UtcNow;
        NotifyStateChanged();
    }

    /// <summary>Records a dashboard load failure with a sanitized message.</summary>
    public void SetDashboardError(string sanitizedError) { _dashboardError = sanitizedError; _isLoadingDashboard = false; NotifyStateChanged(); }

    /// <summary>Changes the active page for the Requirements tab.</summary>
    public void GoToRequirementsPage(int page) { _requirementsPage = Clamp(page, RequirementsTotalPages); NotifyStateChanged(); }

    /// <summary>Changes the active page for the Obligations tab.</summary>
    public void GoToObligationsPage(int page) { _obligationsPage = Clamp(page, ObligationsTotalPages); NotifyStateChanged(); }

    /// <summary>Changes the active page for the Tracking tab.</summary>
    public void GoToTrackingPage(int page) { _trackingPage = Clamp(page, TrackingTotalPages); NotifyStateChanged(); }

    /// <summary>Attempts to resolve a detail record from the in-memory cache populated by the last dashboard load.</summary>
    /// <returns>True when found in cache (no network round-trip needed).</returns>
    public bool TryGetCachedDetail(ComplianceCategory category, Guid itemId, out ComplianceDetailViewModel? detail) =>
        _detailCache.TryGetValue((category, itemId), out detail);

    /// <summary>Marks the detail panel as loading.</summary>
    public void BeginDetailLoad() { _isLoadingDetail = true; _detailError = null; NotifyStateChanged(); }

    /// <summary>Applies the resolved detail record (from cache or a full dashboard reload).</summary>
    public void SetSelectedItem(ComplianceDetailViewModel item) { SelectedItem = item; _isLoadingDetail = false; _detailError = null; NotifyStateChanged(); }

    /// <summary>Records a detail load failure.</summary>
    public void SetDetailError(string sanitizedError) { _detailError = sanitizedError; _isLoadingDetail = false; NotifyStateChanged(); }

    /// <summary>Clears the selected detail record.</summary>
    public void ClearSelection() { SelectedItem = null; NotifyStateChanged(); }

    private static IReadOnlyList<ComplianceRowViewModel> Page(IReadOnlyList<ComplianceRowViewModel> source, int page) =>
        source.Skip((page - 1) * PageSize).Take(PageSize).ToList();

    private static int TotalPages(IReadOnlyList<ComplianceRowViewModel> source) =>
        Math.Max(1, (int)Math.Ceiling(source.Count / (double)PageSize));

    private static int Clamp(int page, int totalPages) => Math.Max(1, Math.Min(page, totalPages));

    private void NotifyStateChanged() => OnChange?.Invoke();
}
