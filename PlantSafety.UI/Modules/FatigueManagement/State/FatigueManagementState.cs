// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Fatigue Management
//   module. Holds the currently loaded date-range-scoped record set (the
//   backend read-model is not paginated — the List page paginates it
//   client-side) plus loading/error state.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;

using PlantSafety.UI.Modules.FatigueManagement.Models;

namespace PlantSafety.UI.Modules.FatigueManagement.State;

/// <summary>Singleton state container for the Fatigue Management module.</summary>
public sealed class FatigueManagementState
{
    private IReadOnlyList<FatigueManagementItemDto> _items = [];
    private bool _isLoading;
    private string? _error;
    private DateTime _generatedAtUtc;

    /// <summary>Gets all fatigue records currently loaded for the active date range.</summary>
    public IReadOnlyList<FatigueManagementItemDto> Items => _items;

    /// <summary>Gets a value indicating whether the record set is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? Error => _error;

    /// <summary>Gets the UTC timestamp when the currently loaded snapshot was generated.</summary>
    public DateTime GeneratedAtUtc => _generatedAtUtc;

    /// <summary>Gets the count of records currently classified High risk.</summary>
    public int HighRiskCount => _items.Count(i => string.Equals(i.FatigueRiskLevel, "High", StringComparison.OrdinalIgnoreCase));

    /// <summary>Gets the count of records currently classified Medium risk.</summary>
    public int MediumRiskCount => _items.Count(i => string.Equals(i.FatigueRiskLevel, "Medium", StringComparison.OrdinalIgnoreCase));

    /// <summary>Gets the count of records currently classified Low risk.</summary>
    public int LowRiskCount => _items.Count(i => string.Equals(i.FatigueRiskLevel, "Low", StringComparison.OrdinalIgnoreCase));

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the record set as loading.</summary>
    public void BeginLoad() { _isLoading = true; _error = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded response.</summary>
    /// <param name="response">The read-model response from the API.</param>
    public void SetItems(FatigueManagementResponseDto response)
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

    /// <summary>Finds a record by its identifier within the currently loaded set, or null if not present.</summary>
    /// <param name="fatigueRecordId">The record identifier to find.</param>
    public FatigueManagementItemDto? Find(Guid fatigueRecordId) =>
        _items.FirstOrDefault(i => i.FatigueRecordId == fatigueRecordId);

    private void NotifyStateChanged() => OnChange?.Invoke();
}
