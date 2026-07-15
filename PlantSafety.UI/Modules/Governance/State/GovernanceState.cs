// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Governance & Access
//   Control module. Reuses the existing PlantSafety.UI.Modules.Admin.Models
//   Users/Roles/Permissions DTOs and PlantSafety.UI.Modules.Admin.ApiClients
//   .AdminApiClient (GET v1/cross-domain/{users,roles,permissions}) rather than
//   duplicating a parallel client, per Intelligence Governance Manual §Reviewer
//   Role Matrix framing — Governance & Access Control surfaces the same
//   platform identity/RBAC read-model as a dedicated module page with its own
//   navigation context and role-drilldown, distinct from Administrative Setup.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;

using PlantSafety.UI.Modules.Admin.Models;

namespace PlantSafety.UI.Modules.Governance.State;

/// <summary>Singleton state container for the Governance &amp; Access Control module.</summary>
public sealed class GovernanceState
{
    private IReadOnlyList<AdminUserDto> _users = [];
    private IReadOnlyList<AdminRoleDto> _roles = [];
    private IReadOnlyList<AdminPermissionDto> _permissions = [];
    private AdminRoleDto? _selectedRole;
    private bool _isLoading;
    private string? _loadError;

    /// <summary>Gets the loaded platform users.</summary>
    public IReadOnlyList<AdminUserDto> Users => _users;

    /// <summary>Gets the loaded security roles.</summary>
    public IReadOnlyList<AdminRoleDto> Roles => _roles;

    /// <summary>Gets the loaded permission grants.</summary>
    public IReadOnlyList<AdminPermissionDto> Permissions => _permissions;

    /// <summary>Gets the currently selected role for the detail drilldown, or null.</summary>
    public AdminRoleDto? SelectedRole => _selectedRole;

    /// <summary>Gets a value indicating whether the governance data is loading.</summary>
    public bool IsLoading => _isLoading;

    /// <summary>Gets a sanitized load error, or null.</summary>
    public string? LoadError => _loadError;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Marks the governance data as loading.</summary>
    public void BeginLoad() { _isLoading = true; _loadError = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded users, roles, and permission grants.</summary>
    /// <param name="users">The loaded user list.</param>
    /// <param name="roles">The loaded role list.</param>
    /// <param name="permissions">The loaded permission grant list.</param>
    public void SetGovernanceData(
        IReadOnlyList<AdminUserDto> users,
        IReadOnlyList<AdminRoleDto> roles,
        IReadOnlyList<AdminPermissionDto> permissions)
    {
        _users = users;
        _roles = roles;
        _permissions = permissions;
        _isLoading = false;
        _loadError = null;
        NotifyStateChanged();
    }

    /// <summary>Records a load failure with a sanitized message.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetLoadError(string sanitizedError) { _loadError = sanitizedError; _isLoading = false; NotifyStateChanged(); }

    /// <summary>Selects a role by identifier from the currently loaded set, if present.</summary>
    /// <param name="roleId">The role identifier to select.</param>
    /// <returns>True if the role was found and selected.</returns>
    public bool SelectRoleById(Guid roleId)
    {
        _selectedRole = _roles.FirstOrDefault(r => r.RoleId == roleId);
        NotifyStateChanged();
        return _selectedRole is not null;
    }

    /// <summary>Gets the permission grants scoped to a given role.</summary>
    /// <param name="roleId">The role identifier.</param>
    public IReadOnlyList<AdminPermissionDto> PermissionsForRole(Guid roleId) =>
        _permissions.Where(p => p.GrantedToRoleId == roleId).ToList();

    /// <summary>Clears the selected role.</summary>
    public void ClearSelection() { _selectedRole = null; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
