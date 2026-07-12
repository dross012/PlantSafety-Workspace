// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single User record.
// SchemaVersion: 1

namespace PlantSafety.Application.CrossDomain.Users.Models;

/// <summary>A single platform user record.</summary>
public sealed record UsersItemDto
{
    /// <summary>User identifier.</summary>
    public required Guid UserId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>User display name.</summary>
    public required string DisplayName { get; init; }

    /// <summary>User email address.</summary>
    public required string Email { get; init; }

    /// <summary>Role currently assigned to the user.</summary>
    public required Guid RoleId { get; init; }

    /// <summary>Current user status (e.g., Active/Locked/Deactivated).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp of the user's last login.</summary>
    public required DateTime LastLoginAtUtc { get; init; }

    /// <summary>Identity of the user who created this account.</summary>
    public required Guid CreatedBy { get; init; }
}
