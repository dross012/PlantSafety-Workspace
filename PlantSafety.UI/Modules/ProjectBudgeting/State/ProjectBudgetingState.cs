// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Project Budgeting
//   module. Holds the current budget row snapshot, the selected project's
//   detail, and loading/error state. Exposes LoadAsync/RefreshAsync
//   orchestration methods that delegate to the injected
//   ProjectBudgetingUiService (see PlanningProjectsState header for the
//   rationale of keeping HTTP-calling collaborators passed as method
//   parameters rather than stored fields).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using PlantSafety.UI.Modules.PlanningProjects.Mappings;
using PlantSafety.UI.Modules.PlanningProjects.Models;
using PlantSafety.UI.Modules.ProjectBudgeting.Models;
using PlantSafety.UI.Modules.ProjectBudgeting.Services;

namespace PlantSafety.UI.Modules.ProjectBudgeting.State;

/// <summary>Singleton state container for the Project Budgeting module.</summary>
public sealed class ProjectBudgetingState
{
    private IReadOnlyList<ProjectBudgetViewModel> _rows = [];
    private decimal _totalPlannedBudget;
    private ProjectDetailViewModel? _selectedProject;
    private bool _isLoadingList;
    private bool _isLoadingDetail;
    private string? _listError;
    private string? _detailError;

    /// <summary>Gets the current snapshot of budget rows. Never null; empty when not loaded.</summary>
    public IReadOnlyList<ProjectBudgetViewModel> Rows => _rows;

    /// <summary>Gets the sum of all known planned-budget figures across <see cref="Rows"/>.</summary>
    public decimal TotalPlannedBudget => _totalPlannedBudget;

    /// <summary>Gets the currently selected project detail, or null when none is selected.</summary>
    public ProjectDetailViewModel? SelectedProject => _selectedProject;

    /// <summary>Gets a value indicating whether the budget overview is loading.</summary>
    public bool IsLoadingList => _isLoadingList;

    /// <summary>Gets a value indicating whether a detail record is loading.</summary>
    public bool IsLoadingDetail => _isLoadingDetail;

    /// <summary>Gets a sanitized list load error, or null.</summary>
    public string? ListError => _listError;

    /// <summary>Gets a sanitized detail load error, or null.</summary>
    public string? DetailError => _detailError;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Loads (or reloads) the tenant's budget overview and applies the mapped result to state.</summary>
    /// <param name="uiService">The Project Budgeting UI orchestration service.</param>
    /// <param name="tenantId">The active tenant identifier.</param>
    /// <param name="fromUtc">Inclusive start of the target-completion date range (UTC).</param>
    /// <param name="toUtc">Inclusive end of the target-completion date range (UTC).</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    public async Task LoadAsync(
        ProjectBudgetingUiService uiService,
        Guid tenantId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default)
    {
        BeginListLoad();

        var (isSuccess, rows, error) = await uiService.LoadBudgetOverviewAsync(tenantId, fromUtc, toUtc, cancellationToken);

        if (isSuccess)
        {
            SetRows(rows);
        }
        else
        {
            SetListError(error ?? "Failed to load the project budget overview.");
        }
    }

    /// <summary>Re-invokes <see cref="LoadAsync"/> with the same parameters. Provided for symmetry with the module Refresh action.</summary>
    public Task RefreshAsync(
        ProjectBudgetingUiService uiService,
        Guid tenantId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default)
        => LoadAsync(uiService, tenantId, fromUtc, toUtc, cancellationToken);

    /// <summary>Loads a single project's full detail for the budgeting detail page.</summary>
    /// <param name="uiService">The Project Budgeting UI orchestration service.</param>
    /// <param name="projectId">The project to load.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    public async Task LoadDetailAsync(ProjectBudgetingUiService uiService, Guid projectId, CancellationToken cancellationToken = default)
    {
        BeginDetailLoad();

        var envelope = await uiService.LoadProjectDetailAsync(projectId, cancellationToken);

        if (envelope.IsSuccess)
        {
            SetSelectedProject(envelope.Data!.ToDetailViewModel());
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

    /// <summary>Marks the budget overview as loading.</summary>
    public void BeginListLoad() { _isLoadingList = true; _listError = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded snapshot of budget rows.</summary>
    /// <param name="rows">The mapped budget row view models.</param>
    public void SetRows(IReadOnlyList<ProjectBudgetViewModel> rows)
    {
        _rows = rows;
        _totalPlannedBudget = ProjectBudgetingUiService.ComputeTotalPlannedBudget(rows);
        _isLoadingList = false;
        _listError = null;
        NotifyStateChanged();
    }

    /// <summary>Records a list load failure with a sanitized message.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetListError(string sanitizedError) { _listError = sanitizedError; _isLoadingList = false; NotifyStateChanged(); }

    /// <summary>Marks the detail panel as loading.</summary>
    public void BeginDetailLoad() { _isLoadingDetail = true; _detailError = null; NotifyStateChanged(); }

    /// <summary>Applies the loaded project detail view model.</summary>
    /// <param name="project">The mapped project detail view model.</param>
    public void SetSelectedProject(ProjectDetailViewModel project) { _selectedProject = project; _isLoadingDetail = false; _detailError = null; NotifyStateChanged(); }

    /// <summary>Records a detail load failure.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetDetailError(string sanitizedError) { _detailError = sanitizedError; _isLoadingDetail = false; NotifyStateChanged(); }

    /// <summary>Clears the selected project (e.g. when navigating away from the detail page).</summary>
    public void ClearSelection() { _selectedProject = null; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
