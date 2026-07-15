// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Policies & Procedures
//   module. Holds the Policies and Procedures list snapshots (client-side
//   paginated/filtered — the backing read-model endpoints return a full
//   date-range result set with no server-side paging), and the selected
//   detail record for either sub-type. Components never call HttpClient
//   directly — they dispatch to this state via PoliciesApiClient.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;

using PlantSafety.UI.Modules.Policies.Mappings;
using PlantSafety.UI.Modules.Policies.Models;

namespace PlantSafety.UI.Modules.Policies.State;

/// <summary>Singleton state container for the Policies &amp; Procedures module.</summary>
public sealed class PoliciesState
{
    private const int PageSize = 10;

    private IReadOnlyList<PolicyDetailViewModel> _policies = [];
    private IReadOnlyList<ProcedureDetailViewModel> _procedures = [];

    private int _policiesPage = 1;
    private int _proceduresPage = 1;
    private string _searchText = string.Empty;

    private bool _isLoadingList;
    private bool _isLoadingDetail;
    private string? _listError;
    private string? _detailError;

    /// <summary>Gets all loaded policies (unfiltered/unpaginated).</summary>
    public IReadOnlyList<PolicyDetailViewModel> Policies => _policies;

    /// <summary>Gets all loaded procedures (unfiltered/unpaginated).</summary>
    public IReadOnlyList<ProcedureDetailViewModel> Procedures => _procedures;

    /// <summary>Gets the current, search-filtered page of policies.</summary>
    public IReadOnlyList<PolicyDetailViewModel> PoliciesPageItems =>
        FilteredPolicies().Skip((_policiesPage - 1) * PageSize).Take(PageSize).ToList();

    /// <summary>Gets the current, search-filtered page of procedures.</summary>
    public IReadOnlyList<ProcedureDetailViewModel> ProceduresPageItems =>
        FilteredProcedures().Skip((_proceduresPage - 1) * PageSize).Take(PageSize).ToList();

    /// <summary>Gets the current 1-based Policies page number.</summary>
    public int PoliciesPage => _policiesPage;

    /// <summary>Gets the current 1-based Procedures page number.</summary>
    public int ProceduresPage => _proceduresPage;

    /// <summary>Gets the total page count for the filtered Policies set.</summary>
    public int PoliciesTotalPages => Math.Max(1, (int)Math.Ceiling(FilteredPolicies().Count / (double)PageSize));

    /// <summary>Gets the total page count for the filtered Procedures set.</summary>
    public int ProceduresTotalPages => Math.Max(1, (int)Math.Ceiling(FilteredProcedures().Count / (double)PageSize));

    /// <summary>Gets the current client-side search text (matched against title/name).</summary>
    public string SearchText => _searchText;

    /// <summary>Gets the currently selected policy detail, or null.</summary>
    public PolicyDetailViewModel? SelectedPolicy { get; private set; }

    /// <summary>Gets the currently selected procedure detail, or null.</summary>
    public ProcedureDetailViewModel? SelectedProcedure { get; private set; }

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

    /// <summary>Applies freshly loaded policy/procedure data.</summary>
    public void SetListData(IReadOnlyList<PolicyItemDto> policies, IReadOnlyList<ProcedureItemDto> procedures)
    {
        _policies = policies.ToViewModels();
        _procedures = procedures.ToViewModels();
        _policiesPage = 1;
        _proceduresPage = 1;
        _isLoadingList = false;
        _listError = null;
        NotifyStateChanged();
    }

    /// <summary>Records a list load failure with a sanitized message.</summary>
    public void SetListError(string sanitizedError) { _listError = sanitizedError; _isLoadingList = false; NotifyStateChanged(); }

    /// <summary>Updates the client-side search filter and resets both tabs to page 1.</summary>
    public void SetSearchText(string text) { _searchText = text ?? string.Empty; _policiesPage = 1; _proceduresPage = 1; NotifyStateChanged(); }

    /// <summary>Changes the active Policies page.</summary>
    public void GoToPoliciesPage(int page) { _policiesPage = Math.Max(1, Math.Min(page, PoliciesTotalPages)); NotifyStateChanged(); }

    /// <summary>Changes the active Procedures page.</summary>
    public void GoToProceduresPage(int page) { _proceduresPage = Math.Max(1, Math.Min(page, ProceduresTotalPages)); NotifyStateChanged(); }

    /// <summary>Attempts to resolve a policy detail from the in-memory cache populated by the last list load.</summary>
    public bool TryGetCachedPolicy(Guid policyId, out PolicyDetailViewModel? detail)
    {
        detail = _policies.FirstOrDefault(p => p.PolicyId == policyId);
        return detail is not null;
    }

    /// <summary>Attempts to resolve a procedure detail from the in-memory cache populated by the last list load.</summary>
    public bool TryGetCachedProcedure(Guid sopId, out ProcedureDetailViewModel? detail)
    {
        detail = _procedures.FirstOrDefault(p => p.SOPId == sopId);
        return detail is not null;
    }

    /// <summary>Marks the detail panel as loading.</summary>
    public void BeginDetailLoad() { _isLoadingDetail = true; _detailError = null; NotifyStateChanged(); }

    /// <summary>Applies the resolved policy detail record.</summary>
    public void SetSelectedPolicy(PolicyDetailViewModel policy) { SelectedPolicy = policy; SelectedProcedure = null; _isLoadingDetail = false; _detailError = null; NotifyStateChanged(); }

    /// <summary>Applies the resolved procedure detail record.</summary>
    public void SetSelectedProcedure(ProcedureDetailViewModel procedure) { SelectedProcedure = procedure; SelectedPolicy = null; _isLoadingDetail = false; _detailError = null; NotifyStateChanged(); }

    /// <summary>Records a detail load failure.</summary>
    public void SetDetailError(string sanitizedError) { _detailError = sanitizedError; _isLoadingDetail = false; NotifyStateChanged(); }

    /// <summary>Clears the selected detail records.</summary>
    public void ClearSelection() { SelectedPolicy = null; SelectedProcedure = null; NotifyStateChanged(); }

    private IReadOnlyList<PolicyDetailViewModel> FilteredPolicies() =>
        string.IsNullOrWhiteSpace(_searchText)
            ? _policies
            : _policies.Where(p => p.PolicyName.Contains(_searchText, StringComparison.OrdinalIgnoreCase)).ToList();

    private IReadOnlyList<ProcedureDetailViewModel> FilteredProcedures() =>
        string.IsNullOrWhiteSpace(_searchText)
            ? _procedures
            : _procedures.Where(p => p.Title.Contains(_searchText, StringComparison.OrdinalIgnoreCase)).ToList();

    private void NotifyStateChanged() => OnChange?.Invoke();
}
