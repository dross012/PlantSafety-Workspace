// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the AI Investigation module.
//   Holds the loaded investigation window, the selected investigation for the
//   detail drilldown, and loading/error state. Components dispatch to this
//   state via AiInvestigationApiClient rather than calling HttpClient directly.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;

using PlantSafety.UI.Modules.AiInvestigation.Models;

namespace PlantSafety.UI.Modules.AiInvestigation.State;

/// <summary>Singleton state container for the AI Investigation module.</summary>
public sealed class AiInvestigationState
{
    private IReadOnlyList<AiInvestigationItemDto> _investigations = [];
    private AiInvestigationItemDto? _selected;
    private bool _isLoading;
    private string? _loadError;
    private DateOnly _rangeStart = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-90));
    private DateOnly _rangeEnd = DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>Gets the currently loaded investigations. Never null; empty when not loaded.</summary>
    public IReadOnlyList<AiInvestigationItemDto> Investigations => _investigations;

    /// <summary>Gets the currently selected investigation for the detail view, or null.</summary>
    public AiInvestigationItemDto? Selected => _selected;

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

    /// <summary>Applies a freshly loaded set of investigations.</summary>
    /// <param name="investigations">The investigation items returned by the API.</param>
    public void SetInvestigations(IReadOnlyList<AiInvestigationItemDto> investigations)
    {
        _investigations = investigations;
        _isLoading = false;
        _loadError = null;
        NotifyStateChanged();
    }

    /// <summary>Records a load failure with a sanitized message.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetLoadError(string sanitizedError) { _loadError = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    /// <summary>Selects an investigation by identifier from the currently loaded set, if present.</summary>
    /// <param name="investigationId">The investigation identifier to select.</param>
    /// <returns>True if the investigation was found and selected.</returns>
    public bool SelectById(Guid investigationId)
    {
        _selected = _investigations.FirstOrDefault(i => i.InvestigationId == investigationId);
        NotifyStateChanged();
        return _selected is not null;
    }

    /// <summary>Clears the selected investigation.</summary>
    public void ClearSelection() { _selected = null; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
