// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Ergonomics module.
//   Holds the currently loaded date-range-scoped record set (the backend
//   read-model is not paginated — the List page paginates it client-side)
//   plus loading/error state.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;

using PlantSafety.UI.Modules.Ergonomics.Models;

namespace PlantSafety.UI.Modules.Ergonomics.State;

/// <summary>Singleton state container for the Ergonomics module.</summary>
public sealed class ErgonomicsState
{
    private IReadOnlyList<ErgonomicsItemDto> _items = [];
    private bool _isLoading;
    private string? _error;
    private DateTime _generatedAtUtc;
    private string _searchText = string.Empty;

    /// <summary>Gets all ergonomics assessments currently loaded for the active date range (unfiltered).</summary>
    public IReadOnlyList<ErgonomicsItemDto> AllItems => _items;

    /// <summary>Gets the current client-side search text (matched against RiskLevel/WorkstationId).</summary>
    public string SearchText => _searchText;

    /// <summary>Gets the items matching the current search text (case-insensitive, client-side).</summary>
    public IReadOnlyList<ErgonomicsItemDto> Items =>
        string.IsNullOrWhiteSpace(_searchText)
            ? _items
            : _items.Where(i =>
                    i.RiskLevel.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ||
                    i.Status.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ||
                    i.WorkstationId.ToString().Contains(_searchText, StringComparison.OrdinalIgnoreCase))
                .ToList();

    /// <summary>Gets a value indicating whether the record set is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? Error => _error;

    /// <summary>Gets the UTC timestamp when the currently loaded snapshot was generated.</summary>
    public DateTime GeneratedAtUtc => _generatedAtUtc;

    /// <summary>Gets the count of records currently classified High risk.</summary>
    public int HighRiskCount => _items.Count(i => string.Equals(i.RiskLevel, "High", StringComparison.OrdinalIgnoreCase));

    /// <summary>Gets the count of records currently classified Medium risk.</summary>
    public int MediumRiskCount => _items.Count(i => string.Equals(i.RiskLevel, "Medium", StringComparison.OrdinalIgnoreCase));

    /// <summary>Gets the count of records currently classified Low risk.</summary>
    public int LowRiskCount => _items.Count(i => string.Equals(i.RiskLevel, "Low", StringComparison.OrdinalIgnoreCase));

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the record set as loading.</summary>
    public void BeginLoad() { _isLoading = true; _error = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded response.</summary>
    /// <param name="response">The read-model response from the API.</param>
    public void SetItems(ErgonomicsResponseDto response)
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

    /// <summary>Updates the client-side search filter (matched against RiskLevel/Status/WorkstationId).</summary>
    /// <param name="text">The new search text.</param>
    public void SetSearchText(string text) { _searchText = text ?? string.Empty; NotifyStateChanged(); }

    /// <summary>Finds a record by its identifier within the currently loaded set, or null if not present.</summary>
    /// <param name="ergonomicsAssessmentId">The record identifier to find.</param>
    public ErgonomicsItemDto? Find(Guid ergonomicsAssessmentId) =>
        _items.FirstOrDefault(i => i.ErgonomicsAssessmentId == ergonomicsAssessmentId);

    private void NotifyStateChanged() => OnChange?.Invoke();
}
