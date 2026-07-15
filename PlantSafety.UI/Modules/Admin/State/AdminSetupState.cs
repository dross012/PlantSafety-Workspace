// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Singleton state container for the Administrative Setup
//   page. Holds independent loading/error/data state for each of the 5
//   data-backed tabs (Users & Roles, Feature Flags, Audit Log — the Tenant
//   and About tabs read directly from TenantState / static app info and
//   need no state of their own). Components never call HttpClient directly —
//   they dispatch to this state via AdminApiClient.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

using PlantSafety.UI.Modules.Admin.Models;

namespace PlantSafety.UI.Modules.Admin.State;

/// <summary>The tabs presented on the Administrative Setup page.</summary>
public enum AdminSetupTab
{
    /// <summary>Tenant / Organization settings tab.</summary>
    Tenant,

    /// <summary>User &amp; Role management tab.</summary>
    UsersAndRoles,

    /// <summary>Feature flags / system options tab.</summary>
    FeatureFlags,

    /// <summary>Integration / webhook settings tab.</summary>
    Integrations,

    /// <summary>Audit log viewer tab.</summary>
    AuditLog,

    /// <summary>About / version info tab.</summary>
    About
}

/// <summary>Singleton state container for the Administrative Setup page.</summary>
public sealed class AdminSetupState
{
    private AdminSetupTab _activeTab = AdminSetupTab.Tenant;

    private IReadOnlyList<AdminUserDto> _users = [];
    private IReadOnlyList<AdminRoleDto> _roles = [];
    private IReadOnlyList<AdminPermissionDto> _permissions = [];
    private IReadOnlyList<AdminSettingDto> _settings = [];
    private IReadOnlyList<AdminAuditLogEntryDto> _auditEntries = [];

    private bool _isLoadingUsersAndRoles;
    private bool _isLoadingFeatureFlags;
    private bool _isLoadingAuditLog;

    private string? _usersAndRolesError;
    private string? _featureFlagsError;
    private string? _auditLogError;

    /// <summary>Gets the currently active tab.</summary>
    public AdminSetupTab ActiveTab => _activeTab;

    /// <summary>Gets the loaded users for the Users &amp; Roles tab.</summary>
    public IReadOnlyList<AdminUserDto> Users => _users;

    /// <summary>Gets the loaded roles for the Users &amp; Roles tab.</summary>
    public IReadOnlyList<AdminRoleDto> Roles => _roles;

    /// <summary>Gets the loaded permission grants for the Users &amp; Roles tab.</summary>
    public IReadOnlyList<AdminPermissionDto> Permissions => _permissions;

    /// <summary>Gets the loaded system settings for the Feature Flags tab.</summary>
    public IReadOnlyList<AdminSettingDto> Settings => _settings;

    /// <summary>Gets the loaded audit log entries for the Audit Log tab.</summary>
    public IReadOnlyList<AdminAuditLogEntryDto> AuditEntries => _auditEntries;

    /// <summary>Gets a value indicating whether the Users &amp; Roles tab is loading.</summary>
    public bool IsLoadingUsersAndRoles => _isLoadingUsersAndRoles;

    /// <summary>Gets a value indicating whether the Feature Flags tab is loading.</summary>
    public bool IsLoadingFeatureFlags => _isLoadingFeatureFlags;

    /// <summary>Gets a value indicating whether the Audit Log tab is loading.</summary>
    public bool IsLoadingAuditLog => _isLoadingAuditLog;

    /// <summary>Gets a sanitized Users &amp; Roles load error, or null.</summary>
    public string? UsersAndRolesError => _usersAndRolesError;

    /// <summary>Gets a sanitized Feature Flags load error, or null.</summary>
    public string? FeatureFlagsError => _featureFlagsError;

    /// <summary>Gets a sanitized Audit Log load error, or null.</summary>
    public string? AuditLogError => _auditLogError;

    /// <summary>Raised whenever the state changes.</summary>
    public event Action? OnChange;

    /// <summary>Switches the active tab.</summary>
    /// <param name="tab">The tab to activate.</param>
    public void SetActiveTab(AdminSetupTab tab)
    {
        _activeTab = tab;
        NotifyStateChanged();
    }

    /// <summary>Marks the Users &amp; Roles tab as loading.</summary>
    public void BeginUsersAndRolesLoad() { _isLoadingUsersAndRoles = true; _usersAndRolesError = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded users and roles.</summary>
    /// <param name="users">The loaded user list.</param>
    /// <param name="roles">The loaded role list.</param>
    /// <param name="permissions">The loaded permission grant list.</param>
    public void SetUsersAndRoles(
        IReadOnlyList<AdminUserDto> users,
        IReadOnlyList<AdminRoleDto> roles,
        IReadOnlyList<AdminPermissionDto> permissions)
    {
        _users = users;
        _roles = roles;
        _permissions = permissions;
        _isLoadingUsersAndRoles = false;
        _usersAndRolesError = null;
        NotifyStateChanged();
    }

    /// <summary>Records a Users &amp; Roles load failure.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetUsersAndRolesError(string sanitizedError) { _usersAndRolesError = sanitizedError; _isLoadingUsersAndRoles = false; NotifyStateChanged(); }

    /// <summary>Marks the Feature Flags tab as loading.</summary>
    public void BeginFeatureFlagsLoad() { _isLoadingFeatureFlags = true; _featureFlagsError = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded system settings.</summary>
    /// <param name="settings">The loaded settings list.</param>
    public void SetSettings(IReadOnlyList<AdminSettingDto> settings)
    {
        _settings = settings;
        _isLoadingFeatureFlags = false;
        _featureFlagsError = null;
        NotifyStateChanged();
    }

    /// <summary>Records a Feature Flags load failure.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetFeatureFlagsError(string sanitizedError) { _featureFlagsError = sanitizedError; _isLoadingFeatureFlags = false; NotifyStateChanged(); }

    /// <summary>Marks the Audit Log tab as loading.</summary>
    public void BeginAuditLogLoad() { _isLoadingAuditLog = true; _auditLogError = null; NotifyStateChanged(); }

    /// <summary>Applies freshly loaded audit log entries.</summary>
    /// <param name="entries">The loaded audit log entries.</param>
    public void SetAuditEntries(IReadOnlyList<AdminAuditLogEntryDto> entries)
    {
        _auditEntries = entries;
        _isLoadingAuditLog = false;
        _auditLogError = null;
        NotifyStateChanged();
    }

    /// <summary>Records an Audit Log load failure.</summary>
    /// <param name="sanitizedError">Safe error message.</param>
    public void SetAuditLogError(string sanitizedError) { _auditLogError = sanitizedError; _isLoadingAuditLog = false; NotifyStateChanged(); }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
