// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// UPDATED: 2026-07-15 — repointed at the new dedicated Environmental Health
//   monitoring-reading read model (EnvironmentalHealthItemDto now represents a
//   reading, not a borrowed Compliance/Policy record).
// FILE DESCRIPTION: Singleton state container for the Environmental Health
//   module. Holds the loaded reading window, the selected reading for the
//   detail drilldown, and loading/error state. Components dispatch to this
//   state via EnvironmentalHealthApiClient.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;

using PlantSafety.UI.Modules.EnvironmentalHealth.Models;

namespace PlantSafety.UI.Modules.EnvironmentalHealth.State;

/// <summary>Singleton state container for the Environmental Health module.</summary>
public sealed class EnvironmentalHealthState
{
    private IReadOnlyList<EnvironmentalHealthItemDto> _readings = [];
    private EnvironmentalHealthItemDto? _selected;
    private bool _isLoading;
    private string? _loadError;
    private DateOnly _rangeStart = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-90));
    private DateOnly _rangeEnd = DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>Gets the currently loaded environmental health readings. Never null; empty when not loaded.</summary>
    public IReadOnlyList<EnvironmentalHealthItemDto> Policies => _readings;

    /// <summary>Gets the currently selected reading for the detail view, or null.</summary>
    public EnvironmentalHealthItemDto? Selected => _selected;

    /// <summary>Gets a value indicating whether the list is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? LoadError => _loadError;

    /// <summary>Gets the inclusive start of the query window.</summary>
    public DateOnly RangeStart => _rangeStart;

    /// <summary>Gets the inclusive end of the query window.</summary>
    public DateOnly RangeEnd => _rangeEnd;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Sets the query date range.</summary>
    public void SetRange(DateOnly start, DateOnly end) { _rangeStart = start; _rangeEnd = end; NotifyStateChanged(); }

    /// <summary>Marks the list as loading.</summary>
    public void BeginLoad() { _isLoading = true; _loadError = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded set of environmental health readings.</summary>
    /// <param name="readings">The reading items returned by the API.</param>
    public void SetPolicies(IReadOnlyList<EnvironmentalHealthItemDto> readings)
    {
        _readings = readings;
        _isLoading = false;
        _loadError = null;
        NotifyStateChanged();
    }

    /// <summary>Records a load failure with a sanitized message.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetLoadError(string sanitizedError) { _loadError = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    /// <summary>Selects a reading by identifier from the currently loaded set, if present.</summary>
    /// <param name="environmentalHealthId">The reading identifier to select.</param>
    /// <returns>True if the reading was found and selected.</returns>
    public bool SelectById(Guid environmentalHealthId)
    {
        _selected = _readings.FirstOrDefault(p => p.EnvironmentalHealthId == environmentalHealthId);
        NotifyStateChanged();
        return _selected is not null;
    }

    /// <summary>Directly sets the selected reading (e.g., after a dedicated GetById call).</summary>
    /// <param name="reading">The reading to select.</param>
    public void SetSelected(EnvironmentalHealthItemDto reading) { _selected = reading; NotifyStateChanged(); }

    /// <summary>Clears the selected reading.</summary>
    public void ClearSelection() { _selected = null; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
