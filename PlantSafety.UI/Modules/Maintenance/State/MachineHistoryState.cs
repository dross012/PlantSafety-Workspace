// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Machine Maintenance
//   History page (Volume M1 §3).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Models.Common;
using PlantSafety.UI.Modules.Maintenance.Models;

namespace PlantSafety.UI.Modules.Maintenance.State;

/// <summary>Singleton state container for the Machine Maintenance History page.</summary>
public sealed class MachineHistoryState
{
    private IReadOnlyList<MaintenanceEventDto> _events = [];
    private bool _isLoading;
    private string? _errorMessage;
    private int _currentPage = 1;
    private int _totalCount;
    private int _totalPages;

    /// <summary>Gets the current page of maintenance events.</summary>
    public IReadOnlyList<MaintenanceEventDto> Events => _events;

    /// <summary>Gets a value indicating whether the page is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? ErrorMessage => _errorMessage;

    /// <summary>Gets the current page number.</summary>
    public int CurrentPage => _currentPage;

    /// <summary>Gets the total matching count.</summary>
    public int TotalCount => _totalCount;

    /// <summary>Gets the total page count.</summary>
    public int TotalPages => _totalPages;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the page as loading.</summary>
    public void BeginLoad() { _isLoading = true; _errorMessage = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded page of events.</summary>
    public void SetEvents(PaginatedResponseDto<MaintenanceEventDto> page)
    {
        _events = page.Items;
        _currentPage = page.PageNumber;
        _totalCount = page.TotalCount;
        _totalPages = page.TotalPages;
        _isLoading = false;
        _errorMessage = null;
        NotifyStateChanged();
    }

    /// <summary>Records a load failure with a sanitized message.</summary>
    public void SetError(string sanitizedError) { _errorMessage = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
