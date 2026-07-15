// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Industrial Hygiene
//   module. Holds the currently loaded date-range-scoped sample set (the
//   backend read-model is not paginated — the List page paginates it
//   client-side) plus loading/error state.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;

using PlantSafety.UI.Modules.IndustrialHygiene.Models;

namespace PlantSafety.UI.Modules.IndustrialHygiene.State;

/// <summary>Singleton state container for the Industrial Hygiene module.</summary>
public sealed class IndustrialHygieneState
{
    private IReadOnlyList<IndustrialHygieneItemDto> _items = [];
    private bool _isLoading;
    private string? _error;
    private DateTime _generatedAtUtc;

    /// <summary>Gets all samples currently loaded for the active date range.</summary>
    public IReadOnlyList<IndustrialHygieneItemDto> Items => _items;

    /// <summary>Gets a value indicating whether the record set is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? Error => _error;

    /// <summary>Gets the UTC timestamp when the currently loaded snapshot was generated.</summary>
    public DateTime GeneratedAtUtc => _generatedAtUtc;

    /// <summary>Gets the count of samples currently classified High exposure.</summary>
    public int HighExposureCount => _items.Count(i => string.Equals(i.ExposureLevel, "High", StringComparison.OrdinalIgnoreCase));

    /// <summary>Gets the count of samples currently classified Medium exposure.</summary>
    public int MediumExposureCount => _items.Count(i => string.Equals(i.ExposureLevel, "Medium", StringComparison.OrdinalIgnoreCase));

    /// <summary>Gets the count of samples currently classified Low exposure.</summary>
    public int LowExposureCount => _items.Count(i => string.Equals(i.ExposureLevel, "Low", StringComparison.OrdinalIgnoreCase));

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the record set as loading.</summary>
    public void BeginLoad() { _isLoading = true; _error = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded response.</summary>
    /// <param name="response">The read-model response from the API.</param>
    public void SetItems(IndustrialHygieneResponseDto response)
    {
        _items = response.Items;
        _generatedAtUtc = response.GeneratedAt;
        _isLoading = false;
        _error = null;
        NotifyStateChanged();
    }

    /// <summary>Records a load failure with a sanitized message.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetError(string sanitizedError) { _error = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    /// <summary>Finds a sample by its identifier within the currently loaded set, or null if not present.</summary>
    /// <param name="industrialHygieneSampleId">The record identifier to find.</param>
    public IndustrialHygieneItemDto? Find(Guid industrialHygieneSampleId) =>
        _items.FirstOrDefault(i => i.IndustrialHygieneSampleId == industrialHygieneSampleId);

    private void NotifyStateChanged() => OnChange?.Invoke();
}
