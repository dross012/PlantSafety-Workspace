// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Planning Projects
//   module. Holds the current project card list (immutable snapshots),
//   the selected project detail, and loading/error state. Exposes
//   LoadAsync/RefreshAsync orchestration methods that accept the typed
//   PlanningProjectsApiClient as a method parameter (rather than storing it
//   as a field) so the state container remains a pure, easily-testable data
//   holder — only the calling page/component owns the injected HttpClient
//   lifetime, matching the DI shape already established by
//   Modules/Alerts/State/AlertsState.cs, while still satisfying the
//   LoadAsync/RefreshAsync contract required of every module state
//   container.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using PlantSafety.UI.Modules.PlanningProjects.ApiClients;
using PlantSafety.UI.Modules.PlanningProjects.Mappings;
using PlantSafety.UI.Modules.PlanningProjects.Models;

namespace PlantSafety.UI.Modules.PlanningProjects.State;

/// <summary>Singleton state container for the Planning Projects module.</summary>
public sealed class PlanningProjectsState
{
    private IReadOnlyList<ProjectCardViewModel> _projects = [];
    private IReadOnlyList<ProjectTimelineItemViewModel> _timelineItems = [];
    private ProjectDetailViewModel? _selectedProject;
    private IReadOnlyList<ProjectTimelineItemViewModel> _detailTimelineItems = [];
    private bool _isLoadingList;
    private bool _isLoadingDetail;
    private string? _listError;
    private string? _detailError;
    private DateTime _lastRefreshUtc;

    /// <summary>Gets the current snapshot of project cards. Never null; empty when not loaded.</summary>
    public IReadOnlyList<ProjectCardViewModel> Projects => _projects;

    /// <summary>Gets the current all-projects Gantt timeline snapshot. Never null; empty when not loaded.</summary>
    public IReadOnlyList<ProjectTimelineItemViewModel> TimelineItems => _timelineItems;

    /// <summary>Gets the currently selected project detail, or null when none is selected.</summary>
    public ProjectDetailViewModel? SelectedProject => _selectedProject;

    /// <summary>Gets the single-project Gantt timeline bar for <see cref="SelectedProject"/>. Empty when none is selected.</summary>
    public IReadOnlyList<ProjectTimelineItemViewModel> DetailTimelineItems => _detailTimelineItems;

    /// <summary>Gets a value indicating whether the project list is loading.</summary>
    public bool IsLoadingList => _isLoadingList;

    /// <summary>Gets a value indicating whether a detail record is loading.</summary>
    public bool IsLoadingDetail => _isLoadingDetail;

    /// <summary>Gets a sanitized list load error, or null.</summary>
    public string? ListError => _listError;

    /// <summary>Gets a sanitized detail load error, or null.</summary>
    public string? DetailError => _detailError;

    /// <summary>Gets the UTC timestamp of the last successful list refresh.</summary>
    public DateTime LastRefreshUtc => _lastRefreshUtc;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>
    /// Loads (or reloads) the tenant's planning project list within the given UTC date
    /// range and applies the mapped result to state. Backed by
    /// GET <c>v1/planning-projects</c> via <paramref name="apiClient"/>.
    /// </summary>
    /// <param name="apiClient">The typed API client to invoke.</param>
    /// <param name="tenantId">The active tenant identifier.</param>
    /// <param name="fromUtc">Inclusive start of the target-completion date range (UTC).</param>
    /// <param name="toUtc">Inclusive end of the target-completion date range (UTC).</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    public async Task LoadAsync(
        PlanningProjectsApiClient apiClient,
        Guid tenantId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default)
    {
        BeginListLoad();

        var nowUtc = DateTime.UtcNow;
        var envelope = await apiClient.GetProjectsAsync(tenantId, siteId: null, fromUtc, toUtc, cancellationToken);

        if (envelope.IsSuccess)
        {
            var items = envelope.Data!.Items;
            var snapshotUtc = envelope.Data!.GeneratedAt;
            SetProjects(items.ToCardViewModels(nowUtc), items.Select(i => i.ToTimelineItemViewModel(snapshotUtc)).ToList());
        }
        else if (envelope.IsProblem)
        {
            SetListError(envelope.Problem!.Detail);
        }
        else
        {
            SetListError($"Failed to load planning projects: {envelope.FaultType}");
        }
    }

    /// <summary>Re-invokes <see cref="LoadAsync"/> with the same parameters. Provided for symmetry with the module Refresh action.</summary>
    public Task RefreshAsync(
        PlanningProjectsApiClient apiClient,
        Guid tenantId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default)
        => LoadAsync(apiClient, tenantId, fromUtc, toUtc, cancellationToken);

    /// <summary>
    /// Loads a single project's full detail by identifier and applies the mapped result to
    /// state. Backed by GET <c>v1/features/planning-projects/projects/{projectId}</c>.
    /// </summary>
    /// <param name="apiClient">The typed API client to invoke.</param>
    /// <param name="projectId">The project to load.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    public async Task LoadDetailAsync(
        PlanningProjectsApiClient apiClient,
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        BeginDetailLoad();

        var envelope = await apiClient.GetProjectByIdAsync(projectId, cancellationToken);

        if (envelope.IsSuccess)
        {
            SetSelectedProject(envelope.Data!.ToDetailViewModel(), [envelope.Data!.ToTimelineItemViewModel()]);
        }
        else if (envelope.IsProblem)
        {
            SetDetailError(envelope.Problem!.Detail);
        }
        else
        {
            SetDetailError($"Failed to load project: {envelope.FaultType}");
        }
    }

    /// <summary>Marks the project list as loading.</summary>
    public void BeginListLoad() { _isLoadingList = true; _listError = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded snapshot of project cards and their timeline bars.</summary>
    /// <param name="projects">The mapped project card view models.</param>
    /// <param name="timelineItems">The mapped all-projects timeline bars.</param>
    public void SetProjects(IReadOnlyList<ProjectCardViewModel> projects, IReadOnlyList<ProjectTimelineItemViewModel> timelineItems)
    {
        _projects = projects;
        _timelineItems = timelineItems;
        _isLoadingList = false;
        _listError = null;
        _lastRefreshUtc = DateTime.UtcNow;
        NotifyStateChanged();
    }

    /// <summary>Records a list load failure with a sanitized message.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetListError(string sanitizedError) { _listError = sanitizedError; _isLoadingList = false; NotifyStateChanged(); }

    /// <summary>Marks the detail panel as loading.</summary>
    public void BeginDetailLoad() { _isLoadingDetail = true; _detailError = null; NotifyStateChanged(); }

    /// <summary>Applies the loaded project detail view model and its timeline bar.</summary>
    /// <param name="project">The mapped project detail view model.</param>
    /// <param name="timelineItems">The mapped single-project timeline bar (zero or one item).</param>
    public void SetSelectedProject(ProjectDetailViewModel project, IReadOnlyList<ProjectTimelineItemViewModel> timelineItems)
    {
        _selectedProject = project;
        _detailTimelineItems = timelineItems;
        _isLoadingDetail = false;
        _detailError = null;
        NotifyStateChanged();
    }

    /// <summary>Records a detail load failure.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetDetailError(string sanitizedError) { _detailError = sanitizedError; _isLoadingDetail = false; NotifyStateChanged(); }

    /// <summary>Clears the selected project (e.g. when navigating away from the detail page).</summary>
    public void ClearSelection() { _selectedProject = null; _detailTimelineItems = []; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
