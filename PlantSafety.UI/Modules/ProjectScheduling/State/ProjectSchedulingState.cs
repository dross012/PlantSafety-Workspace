// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Project Scheduling
//   module. Holds the current all-projects Gantt timeline snapshot, the
//   target-completion schedule event list, the selected project's detail
//   and its own timeline/schedule events, and loading/error state. Exposes
//   LoadAsync/RefreshAsync orchestration methods that delegate to the
//   injected ProjectSchedulingUiService (see PlanningProjectsState header
//   for the rationale of keeping HTTP-calling collaborators passed as
//   method parameters rather than stored fields).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using PlantSafety.UI.Modules.PlanningProjects.Mappings;
using PlantSafety.UI.Modules.PlanningProjects.Models;
using PlantSafety.UI.Modules.ProjectScheduling.Services;

namespace PlantSafety.UI.Modules.ProjectScheduling.State;

/// <summary>Singleton state container for the Project Scheduling module.</summary>
public sealed class ProjectSchedulingState
{
    private IReadOnlyList<ProjectTimelineItemViewModel> _timelineItems = [];
    private IReadOnlyList<ScheduleEventViewModel> _events = [];
    private ProjectDetailViewModel? _selectedProject;
    private IReadOnlyList<ProjectTimelineItemViewModel> _detailTimelineItems = [];
    private IReadOnlyList<ScheduleEventViewModel> _detailEvents = [];
    private bool _isLoadingList;
    private bool _isLoadingDetail;
    private string? _listError;
    private string? _detailError;

    /// <summary>Gets the current all-projects Gantt timeline snapshot. Never null; empty when not loaded.</summary>
    public IReadOnlyList<ProjectTimelineItemViewModel> TimelineItems => _timelineItems;

    /// <summary>Gets the current target-completion schedule events. Never null; empty when not loaded.</summary>
    public IReadOnlyList<ScheduleEventViewModel> Events => _events;

    /// <summary>Gets the currently selected project detail, or null when none is selected.</summary>
    public ProjectDetailViewModel? SelectedProject => _selectedProject;

    /// <summary>Gets the single-project Gantt timeline bar for <see cref="SelectedProject"/>. Empty when none is selected.</summary>
    public IReadOnlyList<ProjectTimelineItemViewModel> DetailTimelineItems => _detailTimelineItems;

    /// <summary>Gets the planned/actual key-date schedule events for <see cref="SelectedProject"/>. Empty when none is selected.</summary>
    public IReadOnlyList<ScheduleEventViewModel> DetailEvents => _detailEvents;

    /// <summary>Gets a value indicating whether the schedule overview is loading.</summary>
    public bool IsLoadingList => _isLoadingList;

    /// <summary>Gets a value indicating whether a detail record is loading.</summary>
    public bool IsLoadingDetail => _isLoadingDetail;

    /// <summary>Gets a sanitized list load error, or null.</summary>
    public string? ListError => _listError;

    /// <summary>Gets a sanitized detail load error, or null.</summary>
    public string? DetailError => _detailError;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Loads (or reloads) the tenant's schedule overview and applies the mapped result to state.</summary>
    /// <param name="uiService">The Project Scheduling UI orchestration service.</param>
    /// <param name="tenantId">The active tenant identifier.</param>
    /// <param name="fromUtc">Inclusive start of the target-completion date range (UTC).</param>
    /// <param name="toUtc">Inclusive end of the target-completion date range (UTC).</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    public async Task LoadAsync(
        ProjectSchedulingUiService uiService,
        Guid tenantId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default)
    {
        BeginListLoad();

        var (isSuccess, timelineItems, events, error) = await uiService.LoadScheduleOverviewAsync(tenantId, fromUtc, toUtc, cancellationToken);

        if (isSuccess)
        {
            SetSchedule(timelineItems, events);
        }
        else
        {
            SetListError(error ?? "Failed to load the project schedule overview.");
        }
    }

    /// <summary>Re-invokes <see cref="LoadAsync"/> with the same parameters. Provided for symmetry with the module Refresh action.</summary>
    public Task RefreshAsync(
        ProjectSchedulingUiService uiService,
        Guid tenantId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default)
        => LoadAsync(uiService, tenantId, fromUtc, toUtc, cancellationToken);

    /// <summary>Loads a single project's full detail for the scheduling detail page.</summary>
    /// <param name="uiService">The Project Scheduling UI orchestration service.</param>
    /// <param name="projectId">The project to load.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    public async Task LoadDetailAsync(ProjectSchedulingUiService uiService, Guid projectId, CancellationToken cancellationToken = default)
    {
        BeginDetailLoad();

        var envelope = await uiService.LoadProjectDetailAsync(projectId, cancellationToken);

        if (envelope.IsSuccess)
        {
            var dto = envelope.Data!;
            SetSelectedProject(dto.ToDetailViewModel(), [dto.ToTimelineItemViewModel()], ProjectSchedulingUiService.ToScheduleEvents(dto));
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

    /// <summary>Marks the schedule overview as loading.</summary>
    public void BeginListLoad() { _isLoadingList = true; _listError = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded snapshot of the schedule overview.</summary>
    /// <param name="timelineItems">The mapped all-projects timeline bars.</param>
    /// <param name="events">The mapped target-completion schedule events.</param>
    public void SetSchedule(IReadOnlyList<ProjectTimelineItemViewModel> timelineItems, IReadOnlyList<ScheduleEventViewModel> events)
    {
        _timelineItems = timelineItems;
        _events = events;
        _isLoadingList = false;
        _listError = null;
        NotifyStateChanged();
    }

    /// <summary>Records a list load failure with a sanitized message.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetListError(string sanitizedError) { _listError = sanitizedError; _isLoadingList = false; NotifyStateChanged(); }

    /// <summary>Marks the detail panel as loading.</summary>
    public void BeginDetailLoad() { _isLoadingDetail = true; _detailError = null; NotifyStateChanged(); }

    /// <summary>Applies the loaded project detail view model and its timeline bar / schedule events.</summary>
    /// <param name="project">The mapped project detail view model.</param>
    /// <param name="timelineItems">The mapped single-project timeline bar (zero or one item).</param>
    /// <param name="events">The mapped planned/actual key-date schedule events.</param>
    public void SetSelectedProject(
        ProjectDetailViewModel project,
        IReadOnlyList<ProjectTimelineItemViewModel> timelineItems,
        IReadOnlyList<ScheduleEventViewModel> events)
    {
        _selectedProject = project;
        _detailTimelineItems = timelineItems;
        _detailEvents = events;
        _isLoadingDetail = false;
        _detailError = null;
        NotifyStateChanged();
    }

    /// <summary>Records a detail load failure.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetDetailError(string sanitizedError) { _detailError = sanitizedError; _isLoadingDetail = false; NotifyStateChanged(); }

    /// <summary>Clears the selected project (e.g. when navigating away from the detail page).</summary>
    public void ClearSelection() { _selectedProject = null; _detailTimelineItems = []; _detailEvents = []; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
