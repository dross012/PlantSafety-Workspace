// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Data Lake module. Holds
//   the loaded activity window, the active zone filter (per the 5 canonical
//   zones), the selected record for the detail drilldown, and loading/error
//   state. Components dispatch to this state via DataLakeApiClient.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;

using PlantSafety.UI.Modules.DataLake.Models;

namespace PlantSafety.UI.Modules.DataLake.State;

/// <summary>Singleton state container for the Data Lake module.</summary>
public sealed class DataLakeState
{
    private IReadOnlyList<DataLakeItemDto> _records = [];
    private DataLakeItemDto? _selected;
    private bool _isLoading;
    private string? _loadError;
    private DateOnly _rangeStart = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-90));
    private DateOnly _rangeEnd = DateOnly.FromDateTime(DateTime.UtcNow);
    private string _zoneFilter = "All";

    /// <summary>Gets the currently loaded data lake activity records. Never null; empty when not loaded.</summary>
    public IReadOnlyList<DataLakeItemDto> Records => _records;

    /// <summary>Gets the records matching the active zone filter.</summary>
    public IReadOnlyList<DataLakeItemDto> FilteredRecords =>
        _zoneFilter == "All"
            ? _records
            : _records.Where(r => string.Equals(r.Zone, _zoneFilter, StringComparison.OrdinalIgnoreCase)).ToList();

    /// <summary>Gets the currently selected record for the detail view, or null.</summary>
    public DataLakeItemDto? Selected => _selected;

    /// <summary>Gets a value indicating whether the list is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? LoadError => _loadError;

    /// <summary>Gets the inclusive start of the query window.</summary>
    public DateOnly RangeStart => _rangeStart;

    /// <summary>Gets the inclusive end of the query window.</summary>
    public DateOnly RangeEnd => _rangeEnd;

    /// <summary>Gets the active zone filter ("All" or one of the 5 canonical zones).</summary>
    public string ZoneFilter => _zoneFilter;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Sets the query date range.</summary>
    public void SetRange(DateOnly start, DateOnly end) { _rangeStart = start; _rangeEnd = end; NotifyStateChanged(); }

    /// <summary>Sets the active zone filter.</summary>
    /// <param name="zone">"All" or one of the 5 canonical zone names.</param>
    public void SetZoneFilter(string zone) { _zoneFilter = zone; NotifyStateChanged(); }

    /// <summary>Marks the list as loading.</summary>
    public void BeginLoad() { _isLoading = true; _loadError = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded set of data lake activity records.</summary>
    /// <param name="records">The record items returned by the API.</param>
    public void SetRecords(IReadOnlyList<DataLakeItemDto> records)
    {
        _records = records;
        _isLoading = false;
        _loadError = null;
        NotifyStateChanged();
    }

    /// <summary>Records a load failure with a sanitized message.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetLoadError(string sanitizedError) { _loadError = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    /// <summary>Selects a record by identifier from the currently loaded set, if present.</summary>
    /// <param name="dataLakeRecordId">The data lake record identifier to select.</param>
    /// <returns>True if the record was found and selected.</returns>
    public bool SelectById(Guid dataLakeRecordId)
    {
        _selected = _records.FirstOrDefault(r => r.DataLakeRecordId == dataLakeRecordId);
        NotifyStateChanged();
        return _selected is not null;
    }

    /// <summary>Clears the selected record.</summary>
    public void ClearSelection() { _selected = null; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
