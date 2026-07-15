// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Contractor Management
//   module. Holds the contractor list snapshot (client-side paginated/
//   filtered — the backing read-model endpoint returns a full date-range
//   result set with no server-side paging), and the selected detail
//   record. Components never call HttpClient directly — they dispatch to
//   this state via ContractorManagementApiClient.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;

using PlantSafety.UI.Modules.ContractorManagement.Models;

namespace PlantSafety.UI.Modules.ContractorManagement.State;

/// <summary>Singleton state container for the Contractor Management module.</summary>
public sealed class ContractorManagementState
{
    private const int PageSize = 10;

    private IReadOnlyList<ContractorViewModel> _contractors = [];
    private int _page = 1;
    private string _searchText = string.Empty;

    private bool _isLoadingList;
    private bool _isLoadingDetail;
    private string? _listError;
    private string? _detailError;

    /// <summary>Gets all loaded contractors (unfiltered/unpaginated).</summary>
    public IReadOnlyList<ContractorViewModel> Contractors => _contractors;

    /// <summary>Gets the current, search-filtered page of contractors.</summary>
    public IReadOnlyList<ContractorViewModel> PageItems =>
        Filtered().Skip((_page - 1) * PageSize).Take(PageSize).ToList();

    /// <summary>Gets the total filtered contractor count.</summary>
    public int FilteredCount => Filtered().Count;

    /// <summary>Gets the total page count for the filtered set.</summary>
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(FilteredCount / (double)PageSize));

    /// <summary>Gets the current 1-based page number.</summary>
    public int CurrentPage => _page;

    /// <summary>Gets the current client-side search text (matched against ContractorName).</summary>
    public string SearchText => _searchText;

    /// <summary>Gets the count of contractors currently Active/Approved.</summary>
    public int ActiveCount => _contractors.Count(c => c.StatusIndicatorState == "Healthy");

    /// <summary>Gets the count of contractors currently Suspended/Rejected/Expired.</summary>
    public int AtRiskCount => _contractors.Count(c => c.StatusIndicatorState == "Critical");

    /// <summary>Gets the currently selected contractor detail, or null.</summary>
    public ContractorViewModel? SelectedContractor { get; private set; }

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

    /// <summary>Applies freshly loaded contractor data.</summary>
    public void SetContractors(IReadOnlyList<ContractorViewModel> contractors)
    {
        _contractors = contractors;
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

    /// <summary>Attempts to resolve a contractor detail from the in-memory cache populated by the last list load.</summary>
    public bool TryGetCachedContractor(Guid contractorRecordId, out ContractorViewModel? detail)
    {
        detail = _contractors.FirstOrDefault(c => c.ContractorRecordId == contractorRecordId);
        return detail is not null;
    }

    /// <summary>Marks the detail panel as loading.</summary>
    public void BeginDetailLoad() { _isLoadingDetail = true; _detailError = null; NotifyStateChanged(); }

    /// <summary>Applies the resolved detail record.</summary>
    public void SetSelectedContractor(ContractorViewModel contractor) { SelectedContractor = contractor; _isLoadingDetail = false; _detailError = null; NotifyStateChanged(); }

    /// <summary>Records a detail load failure.</summary>
    public void SetDetailError(string sanitizedError) { _detailError = sanitizedError; _isLoadingDetail = false; NotifyStateChanged(); }

    /// <summary>Clears the selected detail record.</summary>
    public void ClearSelection() { SelectedContractor = null; NotifyStateChanged(); }

    private IReadOnlyList<ContractorViewModel> Filtered() =>
        string.IsNullOrWhiteSpace(_searchText)
            ? _contractors
            : _contractors.Where(c => c.ContractorName.Contains(_searchText, StringComparison.OrdinalIgnoreCase)).ToList();

    private void NotifyStateChanged() => OnChange?.Invoke();
}
