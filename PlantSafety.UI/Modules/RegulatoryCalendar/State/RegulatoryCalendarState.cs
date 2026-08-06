// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Regulatory Calendar
//   module. Holds the merged, date-sorted entry list across all nine source
//   endpoints, the active date-range filter, client-side search/group
//   filters and pagination, and the selected detail record. Components
//   never call HttpClient directly — they dispatch to this state via
//   RegulatoryCalendarApiClient, which is injected by the pages that
//   trigger loads.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;

using PlantSafety.UI.Modules.RegulatoryCalendar.Models;

namespace PlantSafety.UI.Modules.RegulatoryCalendar.State;

/// <summary>Singleton state container for the Regulatory Calendar module.</summary>
public sealed class RegulatoryCalendarState
{
    private const int PageSize = 15;

    private IReadOnlyList<CalendarEntryViewModel> _entries = [];
    private int _page = 1;
    private string _searchText = string.Empty;
    private CalendarEntryGroup? _groupFilter;

    private bool _isLoadingList;
    private bool _isLoadingDetail;
    private string? _listError;
    private string? _detailError;

    private DateOnly _rangeStart = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
    private DateOnly _rangeEnd = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(180));

    /// <summary>Gets all loaded, merged calendar entries (unfiltered), sorted by date ascending.</summary>
    public IReadOnlyList<CalendarEntryViewModel> Entries => _entries;

    /// <summary>Gets the current, filtered page of calendar entries.</summary>
    public IReadOnlyList<CalendarEntryViewModel> PageItems =>
        Filtered().Skip((_page - 1) * PageSize).Take(PageSize).ToList();

    /// <summary>Gets the total filtered entry count.</summary>
    public int FilteredCount => Filtered().Count;

    /// <summary>Gets the total page count for the filtered set.</summary>
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(FilteredCount / (double)PageSize));

    /// <summary>Gets the current 1-based page number.</summary>
    public int CurrentPage => _page;

    /// <summary>Gets the current client-side search text (matched against Label).</summary>
    public string SearchText => _searchText;

    /// <summary>Gets the current group filter, or null to show all groups.</summary>
    public CalendarEntryGroup? GroupFilter => _groupFilter;

    /// <summary>Gets the inclusive UTC date-range start applied to the last fetch.</summary>
    public DateOnly RangeStart => _rangeStart;

    /// <summary>Gets the inclusive UTC date-range end applied to the last fetch.</summary>
    public DateOnly RangeEnd => _rangeEnd;

    /// <summary>Gets the count of entries currently Overdue/Expired/Failed/Missed.</summary>
    public int CriticalCount => _entries.Count(e => e.StatusIndicatorState == "Critical");

    /// <summary>Gets the count of entries due within the next 7 days.</summary>
    public int DueThisWeekCount => _entries.Count(e => e.DateUtc <= DateTime.UtcNow.AddDays(7) && e.StatusIndicatorState != "Offline");

    /// <summary>Gets the currently selected detail record, or null.</summary>
    public CalendarEntryViewModel? SelectedEntry { get; private set; }

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

    /// <summary>Applies a freshly loaded, merged set of calendar entries across all nine sources.</summary>
    public void SetEntries(IEnumerable<CalendarEntryViewModel> entries)
    {
        _entries = entries.OrderBy(e => e.DateUtc).ToList();
        _page = 1;
        _isLoadingList = false;
        _listError = null;
        NotifyStateChanged();
    }

    /// <summary>Records a list load failure with a sanitized message.</summary>
    public void SetListError(string sanitizedError) { _listError = sanitizedError; _isLoadingList = false; NotifyStateChanged(); }

    /// <summary>Updates the client-side search filter and resets to page 1.</summary>
    public void SetSearchText(string text) { _searchText = text ?? string.Empty; _page = 1; NotifyStateChanged(); }

    /// <summary>Updates the group filter (Deadline/Inspection/Renewal/all) and resets to page 1.</summary>
    public void SetGroupFilter(CalendarEntryGroup? group) { _groupFilter = group; _page = 1; NotifyStateChanged(); }

    /// <summary>Updates the active date range (triggers a re-fetch from the calling page).</summary>
    public void SetDateRange(DateOnly start, DateOnly end) { _rangeStart = start; _rangeEnd = end; NotifyStateChanged(); }

    /// <summary>Changes the active page.</summary>
    public void GoToPage(int page) { _page = Math.Max(1, Math.Min(page, TotalPages)); NotifyStateChanged(); }

    /// <summary>Attempts to resolve an entry detail from the in-memory cache populated by the last list load.</summary>
    public bool TryGetCachedEntry(CalendarEntrySource source, Guid entryId, out CalendarEntryViewModel? entry)
    {
        entry = _entries.FirstOrDefault(e => e.Source == source && e.EntryId == entryId);
        return entry is not null;
    }

    /// <summary>Marks the detail panel as loading.</summary>
    public void BeginDetailLoad() { _isLoadingDetail = true; _detailError = null; NotifyStateChanged(); }

    /// <summary>Applies the resolved detail record.</summary>
    public void SetSelectedEntry(CalendarEntryViewModel entry) { SelectedEntry = entry; _isLoadingDetail = false; _detailError = null; NotifyStateChanged(); }

    /// <summary>Records a detail load failure.</summary>
    public void SetDetailError(string sanitizedError) { _detailError = sanitizedError; _isLoadingDetail = false; NotifyStateChanged(); }

    /// <summary>Clears the selected detail record.</summary>
    public void ClearSelection() { SelectedEntry = null; NotifyStateChanged(); }

    private IReadOnlyList<CalendarEntryViewModel> Filtered()
    {
        IEnumerable<CalendarEntryViewModel> query = _entries;

        if (_groupFilter.HasValue)
        {
            query = query.Where(e => e.Group == _groupFilter.Value);
        }

        if (!string.IsNullOrWhiteSpace(_searchText))
        {
            query = query.Where(e => e.Label.Contains(_searchText, StringComparison.OrdinalIgnoreCase));
        }

        return query.ToList();
    }

    // ============================================================
    // Compliance.RegulatoryCalendar write/lifecycle surface state — Create -> Activate ->
    // RecordCompliance -> Close -> Archive. Distinct generic RegulatoryCalendar aggregate from
    // the 9 read-only Deadlines/Inspections/Renewals source verticals above.
    // ============================================================

    private RegulatoryCalendarItemDto? _selectedRecord;
    private bool _isLoadingRecord;
    private string? _recordError;
    private bool _isBusy;
    private string? _actionMessage;
    private string? _actionError;
    private bool _isCreating;
    private string? _createError;

    /// <summary>Gets the currently loaded generic RegulatoryCalendar write-surface record, or null.</summary>
    public RegulatoryCalendarItemDto? SelectedRecord => _selectedRecord;

    /// <summary>Gets a value indicating whether the generic record is loading.</summary>
    public bool IsLoadingRecord => _isLoadingRecord;

    /// <summary>Gets a sanitized generic record load error, or null.</summary>
    public string? RecordError => _recordError;

    /// <summary>Gets a value indicating whether a lifecycle action is in flight.</summary>
    public bool IsBusy => _isBusy;

    /// <summary>Gets the last lifecycle action's informational message, or null.</summary>
    public string? ActionMessage => _actionMessage;

    /// <summary>Gets the last lifecycle action's error message, or null.</summary>
    public string? ActionError => _actionError;

    /// <summary>Gets a value indicating whether a create operation is in flight.</summary>
    public bool IsCreating => _isCreating;

    /// <summary>Gets the last create operation's error message, or null.</summary>
    public string? CreateError => _createError;

    /// <summary>Marks the generic record as loading.</summary>
    public void BeginRecordLoad() { _isLoadingRecord = true; _recordError = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded generic RegulatoryCalendar record.</summary>
    public void SetSelectedRecord(RegulatoryCalendarItemDto record) { _selectedRecord = record; _isLoadingRecord = false; _recordError = null; NotifyStateChanged(); }

    /// <summary>Records a generic record load failure with a sanitized message.</summary>
    public void SetRecordError(string sanitizedError) { _recordError = sanitizedError; _isLoadingRecord = false; NotifyStateChanged(); }

    /// <summary>Marks a lifecycle action as in flight.</summary>
    public void BeginAction() { _isBusy = true; _actionError = null; NotifyStateChanged(); }

    /// <summary>Records a successful lifecycle action's informational message.</summary>
    public void CompleteAction(string message) { _isBusy = false; _actionMessage = message; _actionError = null; NotifyStateChanged(); }

    /// <summary>Records a failed lifecycle action with a sanitized message.</summary>
    public void SetActionError(string sanitizedError) { _isBusy = false; _actionError = sanitizedError; NotifyStateChanged(); }

    /// <summary>Marks a create operation as in flight.</summary>
    public void BeginCreate() { _isCreating = true; _createError = null; NotifyStateChanged(); }

    /// <summary>Records a failed create operation with a sanitized message.</summary>
    public void SetCreateError(string sanitizedError) { _isCreating = false; _createError = sanitizedError; NotifyStateChanged(); }

    /// <summary>Resets create-form state.</summary>
    public void ResetCreate() { _isCreating = false; _createError = null; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
