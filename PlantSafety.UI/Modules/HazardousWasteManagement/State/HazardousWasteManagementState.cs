// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-04
// FILE DESCRIPTION: Singleton state container for the HazardousWasteManagement module. Holds the
//   open-manifests set (Hazardous Waste Lifecycle Tracker) and the accumulation-areas-nearing-
//   time-limit set (Accumulation Area Time-Limit Board), derives client-side search filtering,
//   and loading/error state. Components never call HttpClient directly — they dispatch to this
//   state via the HazardousWasteManagementApiClient, which is injected by components that
//   trigger loads.
// SchemaVersion: 2026.2
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;

using PlantSafety.UI.Modules.HazardousWasteManagement.Models;

namespace PlantSafety.UI.Modules.HazardousWasteManagement.State;

/// <summary>Singleton state container for the HazardousWasteManagement module dashboards.</summary>
public sealed class HazardousWasteManagementState
{
    private IReadOnlyList<HazardousWasteManifestDto> _openManifests = [];
    private IReadOnlyList<AccumulationAreaDto> _areasNearingTimeLimit = [];
    private string _manifestSearchText = string.Empty;
    private bool _isLoadingManifests;
    private bool _isLoadingAreas;
    private string? _manifestsError;
    private string? _areasError;

    /// <summary>Gets the full set of open manifests for the current tenant/site.</summary>
    public IReadOnlyList<HazardousWasteManifestDto> OpenManifests => _openManifests;

    /// <summary>Gets the full set of accumulation areas nearing their regulatory time limit.</summary>
    public IReadOnlyList<AccumulationAreaDto> AreasNearingTimeLimit => _areasNearingTimeLimit;

    /// <summary>Gets the current client-side manifest search text.</summary>
    public string ManifestSearchText => _manifestSearchText;

    /// <summary>Gets a value indicating whether the open-manifests list is loading.</summary>
    public bool IsLoadingManifests => _isLoadingManifests;

    /// <summary>Gets a value indicating whether the areas-nearing-time-limit list is loading.</summary>
    public bool IsLoadingAreas => _isLoadingAreas;

    /// <summary>Gets a sanitized manifests load error, or null.</summary>
    public string? ManifestsError => _manifestsError;

    /// <summary>Gets a sanitized areas load error, or null.</summary>
    public string? AreasError => _areasError;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Gets the open manifests matching the current search text (case-insensitive, client-side).</summary>
    public IReadOnlyList<HazardousWasteManifestDto> FilteredManifests =>
        string.IsNullOrWhiteSpace(_manifestSearchText)
            ? _openManifests
            : _openManifests.Where(i =>
                    i.ManifestNumber.Contains(_manifestSearchText, StringComparison.OrdinalIgnoreCase) ||
                    i.Status.Contains(_manifestSearchText, StringComparison.OrdinalIgnoreCase))
                .ToList();

    /// <summary>Marks the manifests list as loading.</summary>
    public void BeginManifestsLoad() { _isLoadingManifests = true; _manifestsError = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded open-manifests set.</summary>
    /// <param name="items">The full set of open manifest records.</param>
    public void SetOpenManifests(IReadOnlyList<HazardousWasteManifestDto> items)
    {
        _openManifests = items;
        _isLoadingManifests = false;
        _manifestsError = null;
        NotifyStateChanged();
    }

    /// <summary>Records an open-manifests load failure with a sanitized message.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetManifestsError(string sanitizedError) { _manifestsError = sanitizedError; _isLoadingManifests = false; NotifyStateChanged(); }

    /// <summary>Updates the client-side manifest search text.</summary>
    /// <param name="text">The new search text.</param>
    public void SetManifestSearchText(string text) { _manifestSearchText = text; NotifyStateChanged(); }

    /// <summary>Marks the areas-nearing-time-limit list as loading.</summary>
    public void BeginAreasLoad() { _isLoadingAreas = true; _areasError = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded areas-nearing-time-limit set.</summary>
    /// <param name="items">The full set of accumulation area records nearing their time limit.</param>
    public void SetAreasNearingTimeLimit(IReadOnlyList<AccumulationAreaDto> items)
    {
        _areasNearingTimeLimit = items;
        _isLoadingAreas = false;
        _areasError = null;
        NotifyStateChanged();
    }

    /// <summary>Records an areas load failure with a sanitized message.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetAreasError(string sanitizedError) { _areasError = sanitizedError; _isLoadingAreas = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
