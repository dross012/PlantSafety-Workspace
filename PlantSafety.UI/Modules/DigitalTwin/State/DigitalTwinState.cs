// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Digital Twin module.
//   Holds the loaded twin asset registry, the selected twin for the detail
//   drilldown, and loading/error state. Components dispatch to this state via
//   DigitalTwinApiClient.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;

using PlantSafety.UI.Modules.DigitalTwin.Models;

namespace PlantSafety.UI.Modules.DigitalTwin.State;

/// <summary>Singleton state container for the Digital Twin module.</summary>
public sealed class DigitalTwinState
{
    private IReadOnlyList<DigitalTwinAssetDto> _twins = [];
    private DigitalTwinAssetDto? _selected;
    private bool _isLoading;
    private string? _loadError;

    /// <summary>Gets the currently loaded digital twin assets. Never null; empty when not loaded.</summary>
    public IReadOnlyList<DigitalTwinAssetDto> Twins => _twins;

    /// <summary>Gets the currently selected twin for the detail view, or null.</summary>
    public DigitalTwinAssetDto? Selected => _selected;

    /// <summary>Gets a value indicating whether the twin registry is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? LoadError => _loadError;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the registry as loading.</summary>
    public void BeginLoad() { _isLoading = true; _loadError = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded set of digital twin assets.</summary>
    /// <param name="twins">The twin items returned by the API.</param>
    public void SetTwins(IReadOnlyList<DigitalTwinAssetDto> twins)
    {
        _twins = twins;
        _isLoading = false;
        _loadError = null;
        NotifyStateChanged();
    }

    /// <summary>Records a load failure with a sanitized message.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetLoadError(string sanitizedError) { _loadError = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    /// <summary>Selects a twin by identifier from the currently loaded set, if present.</summary>
    /// <param name="twinId">The twin identifier to select.</param>
    /// <returns>True if the twin was found and selected.</returns>
    public bool SelectById(Guid twinId)
    {
        _selected = _twins.FirstOrDefault(t => t.TwinId == twinId);
        NotifyStateChanged();
        return _selected is not null;
    }

    /// <summary>Clears the selected twin.</summary>
    public void ClearSelection() { _selected = null; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
