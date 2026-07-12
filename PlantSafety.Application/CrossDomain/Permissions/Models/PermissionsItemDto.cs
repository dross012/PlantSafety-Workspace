// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single Permission grant.
// SchemaVersion: 1

namespace PlantSafety.Application.CrossDomain.Permissions.Models;

/// <summary>A single permission grant record.</summary>
public sealed record PermissionsItemDto
{
    /// <summary>Permission identifier.</summary>
    public required Guid PermissionId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Permission name (e.g., Modules.Compliance.Read).</summary>
    public required string PermissionName { get; init; }

    /// <summary>Permission scope (e.g., Tenant/Site/Module).</summary>
    public required string Scope { get; init; }

    /// <summary>Role the permission is granted to.</summary>
    public required Guid GrantedToRoleId { get; init; }

    /// <summary>Current grant status (e.g., Active/Revoked/PendingApproval).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the permission was granted.</summary>
    public required DateTime GrantedAtUtc { get; init; }

    /// <summary>Identity of the user who granted the permission.</summary>
    public required Guid GrantedBy { get; init; }
}
