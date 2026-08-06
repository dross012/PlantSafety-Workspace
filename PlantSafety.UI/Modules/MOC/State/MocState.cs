// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Management of Change
//   module. Holds the workflow list snapshot (client-side paginated — the
//   backing read-model endpoint returns a full date-range result set with
//   no server-side paging), the raw approvals/risk-assessments caches used
//   to build the detail page's related tabs, and the selected detail
//   record. Components never call HttpClient directly — they dispatch to
//   this state via MocApiClient, which is injected by the pages that
//   trigger loads.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;

using PlantSafety.UI.Modules.MOC.Mappings;
using PlantSafety.UI.Modules.MOC.Models;

namespace PlantSafety.UI.Modules.MOC.State;

/// <summary>Singleton state container for the Management of Change module.</summary>
public sealed class MocState
{
    private const int PageSize = 10;

    private IReadOnlyList<MocWorkflowItemDto> _workflows = [];
    private IReadOnlyList<MocApprovalItemDto> _approvals = [];
    private IReadOnlyList<MocRiskAssessmentItemDto> _riskAssessments = [];

    private IReadOnlyList<MocRowViewModel> _rows = [];
    private int _page = 1;
    private string _searchText = string.Empty;

    private bool _isLoadingList;
    private bool _isLoadingDetail;
    private string? _listError;
    private string? _detailError;

    private bool _isCreatingMoc;
    private string? _createMocError;
    private Guid? _createdMocId;

    /// <summary>Gets the current page of workflow rows, filtered by <see cref="SearchText"/>.</summary>
    public IReadOnlyList<MocRowViewModel> PageItems => Filtered().Skip((_page - 1) * PageSize).Take(PageSize).ToList();

    /// <summary>Gets the total filtered row count.</summary>
    public int FilteredCount => Filtered().Count;

    /// <summary>Gets the total page count for the filtered set.</summary>
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(FilteredCount / (double)PageSize));

    /// <summary>Gets the current 1-based page number.</summary>
    public int CurrentPage => _page;

    /// <summary>Gets or sets the current client-side search text (matched against ChangeTitle).</summary>
    public string SearchText => _searchText;

    /// <summary>Gets the count of workflows currently Open.</summary>
    public int OpenCount => _workflows.Count(w => w.Status.Equals("Open", StringComparison.OrdinalIgnoreCase));

    /// <summary>Gets the count of workflows currently OnHold.</summary>
    public int OnHoldCount => _workflows.Count(w => w.Status.Equals("OnHold", StringComparison.OrdinalIgnoreCase));

    /// <summary>Gets the count of workflows currently Completed.</summary>
    public int CompletedCount => _workflows.Count(w => w.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase));

    /// <summary>Gets the currently selected detail record, or null when none is selected.</summary>
    public MocDetailViewModel? SelectedWorkflow { get; private set; }

    /// <summary>Gets a value indicating whether the list is loading.</summary>
    public bool IsLoadingList => _isLoadingList;

    /// <summary>Gets a value indicating whether a detail record is loading.</summary>
    public bool IsLoadingDetail => _isLoadingDetail;

    /// <summary>Gets a sanitized list load error, or null.</summary>
    public string? ListError => _listError;

    /// <summary>Gets a sanitized detail load error, or null.</summary>
    public string? DetailError => _detailError;

    /// <summary>Gets a value indicating whether a Create MOC submission is in flight.</summary>
    public bool IsCreatingMoc => _isCreatingMoc;

    /// <summary>Gets a sanitized Create MOC submission error, or null.</summary>
    public string? CreateMocError => _createMocError;

    /// <summary>Gets the identifier of the most recently created MOC request, or null.</summary>
    public Guid? CreatedMocId => _createdMocId;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the list as loading.</summary>
    public void BeginListLoad() { _isLoadingList = true; _listError = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded workflow/approval/risk-assessment data.</summary>
    public void SetListData(
        IReadOnlyList<MocWorkflowItemDto> workflows,
        IReadOnlyList<MocApprovalItemDto> approvals,
        IReadOnlyList<MocRiskAssessmentItemDto> riskAssessments)
    {
        _workflows = workflows;
        _approvals = approvals;
        _riskAssessments = riskAssessments;
        _rows = workflows.ToRowViewModels();
        _page = 1;
        _isLoadingList = false;
        _listError = null;
        NotifyStateChanged();
    }

    /// <summary>Records a list load failure with a sanitized message.</summary>
    public void SetListError(string sanitizedError) { _listError = sanitizedError; _isLoadingList = false; NotifyStateChanged(); }

    /// <summary>Updates the client-side search filter and resets to page 1.</summary>
    public void SetSearchText(string text) { _searchText = text ?? string.Empty; _page = 1; NotifyStateChanged(); }

    /// <summary>Changes the active page.</summary>
    public void GoToPage(int page) { _page = Math.Max(1, Math.Min(page, TotalPages)); NotifyStateChanged(); }

    /// <summary>Attempts to resolve a workflow detail from the in-memory cache populated by the last list load.</summary>
    public bool TryGetCachedDetail(Guid workflowId, out MocDetailViewModel? detail)
    {
        var workflow = _workflows.FirstOrDefault(w => w.WorkflowId == workflowId);
        if (workflow is null)
        {
            detail = null;
            return false;
        }

        detail = workflow.ToDetailViewModel(_approvals, _riskAssessments);
        return true;
    }

    /// <summary>Marks the detail panel as loading.</summary>
    public void BeginDetailLoad() { _isLoadingDetail = true; _detailError = null; NotifyStateChanged(); }

    /// <summary>Applies the resolved detail record.</summary>
    public void SetSelectedWorkflow(MocDetailViewModel workflow) { SelectedWorkflow = workflow; _isLoadingDetail = false; _detailError = null; NotifyStateChanged(); }

    /// <summary>Records a detail load failure.</summary>
    public void SetDetailError(string sanitizedError) { _detailError = sanitizedError; _isLoadingDetail = false; NotifyStateChanged(); }

    /// <summary>Clears the selected detail record.</summary>
    public void ClearSelection() { SelectedWorkflow = null; NotifyStateChanged(); }

    /// <summary>Marks a Create MOC submission as in flight.</summary>
    public void BeginCreateMoc() { _isCreatingMoc = true; _createMocError = null; _createdMocId = null; NotifyStateChanged(); }

    /// <summary>Records a successful Create MOC submission.</summary>
    public void SetCreateMocSuccess(Guid mocId) { _createdMocId = mocId; _isCreatingMoc = false; _createMocError = null; NotifyStateChanged(); }

    /// <summary>Records a Create MOC submission failure with a sanitized message.</summary>
    public void SetCreateMocError(string sanitizedError) { _createMocError = sanitizedError; _isCreatingMoc = false; NotifyStateChanged(); }

    /// <summary>Resets Create MOC submission state (e.g. when navigating to the form).</summary>
    public void ResetCreateMoc() { _isCreatingMoc = false; _createMocError = null; _createdMocId = null; NotifyStateChanged(); }

    private IReadOnlyList<MocRowViewModel> Filtered() =>
        string.IsNullOrWhiteSpace(_searchText)
            ? _rows
            : _rows.Where(r => r.ChangeTitle.Contains(_searchText, StringComparison.OrdinalIgnoreCase)).ToList();

    private void NotifyStateChanged() => OnChange?.Invoke();
}
