// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single Role record.
// SchemaVersion: 1

namespace PlantSafety.Application.CrossDomain.Roles.Models;

/// <summary>A single security role record.</summary>
public sealed record RolesItemDto
{
    /// <summary>Role identifier.</summary>
    public required Guid RoleId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Role display name.</summary>
    public required string RoleName { get; init; }

    /// <summary>Role description.</summary>
    public required string Description { get; init; }

    /// <summary>Number of permissions currently assigned to the role.</summary>
    public required int PermissionCount { get; init; }

    /// <summary>Current role status (e.g., Active/Deprecated/Draft).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the role was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created the role.</summary>
    public required Guid CreatedBy { get; init; }
}
