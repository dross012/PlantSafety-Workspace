// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-03
// FILE DESCRIPTION: Singleton state container for the Admin Portal's User Management page.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Modules.Admin.Models;

namespace PlantSafety.UI.Modules.Admin.State;

/// <summary>Singleton state container for the Admin Portal's User Management page.</summary>
public sealed class UserManagementState
{
    private IReadOnlyList<UserProfileDto> _users = [];
    private int _totalCount;
    private int _pageNumber = 1;
    private int _pageSize = 25;
    private bool _isLoading;
    private bool _isCreating;
    private string? _error;
    private string? _actionMessage;

    /// <summary>Gets the currently loaded page of users.</summary>
    public IReadOnlyList<UserProfileDto> Users => _users;

    /// <summary>Gets the total number of users matching the current filters, across all pages.</summary>
    public int TotalCount => _totalCount;

    /// <summary>Gets the current 1-based page number.</summary>
    public int PageNumber => _pageNumber;

    /// <summary>Gets the current page size.</summary>
    public int PageSize => _pageSize;

    /// <summary>Gets a value indicating whether the user list is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a value indicating whether a create-user action is in flight.</summary>
    public bool IsCreating => _isCreating;

    /// <summary>Gets a sanitized list load error, or null.</summary>
    public string? Error => _error;

    /// <summary>Gets the most recent action outcome message (success or sanitized error).</summary>
    public string? ActionMessage => _actionMessage;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the user list as loading.</summary>
    public void BeginLoad() { _isLoading = true; _error = null; NotifyStateChanged(); }

    /// <summary>Applies a freshly loaded page of users.</summary>
    public void SetUsers(PagedUsersDto page)
    {
        _users = page.Items;
        _totalCount = page.TotalCount;
        _pageNumber = page.PageNumber;
        _pageSize = page.PageSize;
        _isLoading = false;
        _error = null;
        NotifyStateChanged();
    }

    /// <summary>Records a user list load failure.</summary>
    public void SetError(string sanitizedError) { _error = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    /// <summary>Marks a create-user action as in flight.</summary>
    public void BeginCreate() { _isCreating = true; _actionMessage = null; NotifyStateChanged(); }

    /// <summary>Records the outcome of a completed create-user action.</summary>
    public void CompleteCreate(string message) { _isCreating = false; _actionMessage = message; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
