// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-side DTO for a User Profile in the EnterpriseFabric module.
//   EnterpriseFabric owns identity, access control, and governance for all platform users.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.EnterpriseFabric.Models.Users;

/// <summary>
/// Read-side projection of a UserProfile aggregate.
/// Represents a platform user with their roles, site assignments, and status.
/// </summary>
public sealed record UserProfileDto
{
    /// <summary>Unique identifier (UUIDv7).</summary>
    public required Guid UserId { get; init; }

    /// <summary>Tenant this user belongs to.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Display name of the user.</summary>
    public required string DisplayName { get; init; }

    /// <summary>Email address (used as login identity).</summary>
    public required string Email { get; init; }

    /// <summary>Assigned roles (e.g., SafetyOfficer, SiteManager, Worker, Admin).</summary>
    public required IReadOnlyList<string> Roles { get; init; }

    /// <summary>Sites this user has access to.</summary>
    public required IReadOnlyList<Guid> SiteIds { get; init; }

    /// <summary>Account status (e.g., Active, Suspended, Deactivated).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp of last login.</summary>
    public DateTimeOffset? LastLoginAtUtc { get; init; }

    /// <summary>UTC timestamp when the account was created.</summary>
    public required DateTimeOffset CreatedAtUtc { get; init; }

    /// <summary>UTC timestamp of last modification.</summary>
    public required DateTimeOffset LastModifiedAtUtc { get; init; }
}
