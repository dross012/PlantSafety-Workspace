// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: UI-side DTOs for the Administrative Setup page. Shapes
//   mirror the existing PlantSafety.Api CrossDomain Users/Roles/Permissions/
//   Administration and Integrations.Auditing read-model item DTOs so
//   AdminApiClient can deserialize real backend responses directly. These
//   read-model endpoints are GET-only scaffolds today — there are no write
//   (command) endpoints yet for user/role assignment, settings, or
//   integrations, so the corresponding Admin Setup UI actions are built
//   against these DTOs but intentionally disabled with a "needs backend
//   wiring" indicator until command endpoints exist (see AdminSetupPage).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.Admin.Models;

/// <summary>Generic list-response envelope shared by all CrossDomain/Integrations read-model endpoints.</summary>
/// <typeparam name="T">The item DTO type.</typeparam>
public sealed class AdminListResponseDto<T>
{
    /// <summary>Gets the tenant the data belongs to.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site the data is scoped to, or null for tenant-wide results.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the UTC timestamp when this read-model snapshot was generated.</summary>
    public DateTime GeneratedAt { get; init; }

    /// <summary>Gets the result items.</summary>
    public IReadOnlyList<T> Items { get; init; } = [];
}

/// <summary>A single platform user record (mirrors <c>UsersItemDto</c>).</summary>
public sealed class AdminUserDto
{
    /// <summary>Gets the user identifier.</summary>
    public Guid UserId { get; init; }

    /// <summary>Gets the user display name.</summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>Gets the user email address.</summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>Gets the role currently assigned to the user.</summary>
    public Guid RoleId { get; init; }

    /// <summary>Gets the current user status (e.g., Active/Locked/Deactivated).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp of the user's last login.</summary>
    public DateTime LastLoginAtUtc { get; init; }
}

/// <summary>A single security role record (mirrors <c>RolesItemDto</c>).</summary>
public sealed class AdminRoleDto
{
    /// <summary>Gets the role identifier.</summary>
    public Guid RoleId { get; init; }

    /// <summary>Gets the role display name.</summary>
    public string RoleName { get; init; } = string.Empty;

    /// <summary>Gets the role description.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Gets the number of permissions currently assigned to the role.</summary>
    public int PermissionCount { get; init; }

    /// <summary>Gets the current role status (e.g., Active/Deprecated/Draft).</summary>
    public string Status { get; init; } = string.Empty;
}

/// <summary>A single permission grant record (mirrors <c>PermissionsItemDto</c>).</summary>
public sealed class AdminPermissionDto
{
    /// <summary>Gets the permission identifier.</summary>
    public Guid PermissionId { get; init; }

    /// <summary>Gets the permission name (e.g., Modules.Compliance.Read).</summary>
    public string PermissionName { get; init; } = string.Empty;

    /// <summary>Gets the permission scope (e.g., Tenant/Site/Module).</summary>
    public string Scope { get; init; } = string.Empty;

    /// <summary>Gets the role the permission is granted to.</summary>
    public Guid GrantedToRoleId { get; init; }

    /// <summary>Gets the current grant status (e.g., Active/Revoked/PendingApproval).</summary>
    public string Status { get; init; } = string.Empty;
}

/// <summary>A single system configuration / feature flag record (mirrors <c>AdministrationItemDto</c>).</summary>
public sealed class AdminSettingDto
{
    /// <summary>Gets the configuration record identifier.</summary>
    public Guid RecordId { get; init; }

    /// <summary>Gets the configuration setting key.</summary>
    public string SettingKey { get; init; } = string.Empty;

    /// <summary>Gets the configuration setting value.</summary>
    public string SettingValue { get; init; } = string.Empty;

    /// <summary>Gets the setting category (e.g., Security/Notifications/Tenancy).</summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>Gets the current record status (e.g., Active/PendingReview/Archived).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp when the record was last modified.</summary>
    public DateTime LastModifiedAtUtc { get; init; }
}

/// <summary>A single integration-layer audit log entry (mirrors <c>AuditingItemDto</c>).</summary>
public sealed class AdminAuditLogEntryDto
{
    /// <summary>Gets the audit log entry identifier.</summary>
    public Guid LogId { get; init; }

    /// <summary>Gets the name of the external system involved in the call.</summary>
    public string ExternalSystem { get; init; } = string.Empty;

    /// <summary>Gets the direction of the call (Inbound/Outbound).</summary>
    public string Direction { get; init; } = string.Empty;

    /// <summary>Gets the endpoint or operation invoked.</summary>
    public string Endpoint { get; init; } = string.Empty;

    /// <summary>Gets the outcome status of the call (e.g., Success/Failed).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp when the call occurred.</summary>
    public DateTime OccurredAtUtc { get; init; }
}
