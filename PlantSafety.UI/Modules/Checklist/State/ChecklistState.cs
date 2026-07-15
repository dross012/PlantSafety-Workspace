// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Checklist module.
//   Holds the checklist list snapshot (client-side paginated/filtered — the
//   backing read-model endpoint returns a full date-range result set with
//   no server-side paging), and the selected detail record. Components
//   never call HttpClient directly — they dispatch to this state via
//   ChecklistApiClient.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;

using PlantSafety.UI.Modules.Checklist.Models;

namespace PlantSafety.UI.Modules.Checklist.State;

/// <summary>Singleton state container for the Checklist module.</summary>
public sealed class ChecklistState
{
    private const int PageSize = 10;

    private IReadOnlyList<ChecklistViewModel> _checklists = [];
    private int _page = 1;
    private string _searchText = string.Empty;

    private bool _isLoadingList;
    private bool _isLoadingDetail;
    private string? _listError;
    private string? _detailError;

    /// <summary>Gets all loaded checklists (unfiltered/unpaginated).</summary>
    public IReadOnlyList<ChecklistViewModel> Checklists => _checklists;

    /// <summary>Gets the current, search-filtered page of checklists.</summary>
    public IReadOnlyList<ChecklistViewModel> PageItems =>
        Filtered().Skip((_page - 1) * PageSize).Take(PageSize).ToList();

    /// <summary>Gets the total filtered checklist count.</summary>
    public int FilteredCount => Filtered().Count;

    /// <summary>Gets the total page count for the filtered set.</summary>
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(FilteredCount / (double)PageSize));

    /// <summary>Gets the current 1-based page number.</summary>
    public int CurrentPage => _page;

    /// <summary>Gets the current client-side search text (matched against Name).</summary>
    public string SearchText => _searchText;

    /// <summary>Gets the count of checklists currently Completed.</summary>
    public int CompletedCount => _checklists.Count(c => c.StatusIndicatorState == "Healthy");

    /// <summary>Gets the count of checklists currently Overdue/Failed.</summary>
    public int OverdueCount => _checklists.Count(c => c.StatusIndicatorState == "Critical");

    /// <summary>Gets the currently selected checklist detail, or null.</summary>
    public ChecklistViewModel? SelectedChecklist { get; private set; }

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

    /// <summary>Applies freshly loaded checklist data.</summary>
    public void SetChecklists(IReadOnlyList<ChecklistViewModel> checklists)
    {
        _checklists = checklists;
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

    /// <summary>Attempts to resolve a checklist detail from the in-memory cache populated by the last list load.</summary>
    public bool TryGetCachedChecklist(Guid checklistId, out ChecklistViewModel? detail)
    {
        detail = _checklists.FirstOrDefault(c => c.ChecklistId == checklistId);
        return detail is not null;
    }

    /// <summary>Marks the detail panel as loading.</summary>
    public void BeginDetailLoad() { _isLoadingDetail = true; _detailError = null; NotifyStateChanged(); }

    /// <summary>Applies the resolved detail record.</summary>
    public void SetSelectedChecklist(ChecklistViewModel checklist) { SelectedChecklist = checklist; _isLoadingDetail = false; _detailError = null; NotifyStateChanged(); }

    /// <summary>Records a detail load failure.</summary>
    public void SetDetailError(string sanitizedError) { _detailError = sanitizedError; _isLoadingDetail = false; NotifyStateChanged(); }

    /// <summary>Clears the selected detail record.</summary>
    public void ClearSelection() { SelectedChecklist = null; NotifyStateChanged(); }

    private IReadOnlyList<ChecklistViewModel> Filtered() =>
        string.IsNullOrWhiteSpace(_searchText)
            ? _checklists
            : _checklists.Where(c => c.Name.Contains(_searchText, StringComparison.OrdinalIgnoreCase)).ToList();

    private void NotifyStateChanged() => OnChange?.Invoke();
}
