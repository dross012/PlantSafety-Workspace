// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-06-17
// FILE DESCRIPTION: Client-side representation of the authenticated user's
//   claims extracted from the Azure AD ID token. Used by UserSessionState
//   and NavMenu to display identity and drive authorization decisions in the UI.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.8
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Models.Auth;

/// <summary>
/// Immutable claims snapshot for the currently authenticated user.
/// Populated once at sign-in and refreshed on token renewal.
/// </summary>
public sealed class UserClaimsDto
{
    /// <summary>Gets the user's unique identifier (Azure AD object ID mapped to platform UserId).</summary>
    public Guid UserId { get; init; }

    /// <summary>Gets the tenant identifier for the authenticated session.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the user's display name for UI presentation.</summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>Gets the user's email address. Never logged or stored in SIEM.</summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>Gets the user's role assignments within the PlantSafety platform.</summary>
    public IReadOnlyList<string> Roles { get; init; } = [];

    /// <summary>Gets the user's module-level permission codes (e.g., "alerts:read", "permits:write").</summary>
    public IReadOnlyList<string> Permissions { get; init; } = [];

    /// <summary>Gets the UTC timestamp when the token expires.</summary>
    public DateTimeOffset TokenExpiresAtUtc { get; init; }

    /// <summary>Represents an unauthenticated (anonymous) user state.</summary>
    public static UserClaimsDto Anonymous => new()
    {
        UserId = Guid.Empty,
        TenantId = Guid.Empty,
        DisplayName = "Anonymous",
        Email = string.Empty,
        TokenExpiresAtUtc = DateTimeOffset.MinValue
    };

    /// <summary>Gets a value indicating whether the user is authenticated (non-empty UserId).</summary>
    public bool IsAuthenticated => UserId != Guid.Empty;

    /// <summary>
    /// Determines whether the user holds the specified role.
    /// </summary>
    /// <param name="role">The role name to check (case-insensitive).</param>
    /// <returns>True if the user has the role; otherwise false.</returns>
    public bool HasRole(string role) =>
        Roles.Contains(role, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether the user holds the specified permission code.
    /// </summary>
    /// <param name="permission">The permission code to check (e.g., "alerts:read").</param>
    /// <returns>True if the user has the permission; otherwise false.</returns>
    public bool HasPermission(string permission) =>
        Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
}
