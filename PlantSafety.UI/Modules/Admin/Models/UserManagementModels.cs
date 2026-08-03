// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-03
// FILE DESCRIPTION: Client-side DTOs for the Admin Portal's User Management page (Phase 7
//   §6.1 Admin Portal). Mirrors PlantSafety.Application.Features.EnterpriseFabric.Models
//   .Users.UserProfileDto and PlantSafety.Api.Features.EnterpriseFabric.Users.Controllers
//   .V1.UsersV1Controller's request/response contracts so UserManagementApiClient can
//   deserialize/serialize real backend shapes directly.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.Admin.Models;

/// <summary>Read-side projection of a platform user (mirrors <c>UserProfileDto</c>).</summary>
public sealed class UserProfileDto
{
    /// <summary>Gets the user's unique identifier.</summary>
    public Guid UserId { get; init; }

    /// <summary>Gets the tenant this user belongs to.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the user's display name.</summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>Gets the user's email address (used as login identity).</summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>Gets the user's assigned roles.</summary>
    public IReadOnlyList<string> Roles { get; init; } = [];

    /// <summary>Gets the sites this user has access to.</summary>
    public IReadOnlyList<Guid> SiteIds { get; init; } = [];

    /// <summary>Gets the account status (e.g. Active, Deactivated).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC instant of the user's last login, if any.</summary>
    public DateTimeOffset? LastLoginAtUtc { get; init; }

    /// <summary>Gets the UTC instant the account was created.</summary>
    public DateTimeOffset CreatedAtUtc { get; init; }

    /// <summary>Gets the UTC instant the account was last modified.</summary>
    public DateTimeOffset LastModifiedAtUtc { get; init; }
}

/// <summary>Paged result set of <see cref="UserProfileDto"/> records (mirrors <c>ListUsersResponse</c>).</summary>
public sealed class PagedUsersDto
{
    /// <summary>Gets the page of users matching the query filters.</summary>
    public IReadOnlyList<UserProfileDto> Items { get; init; } = [];

    /// <summary>Gets the total number of users matching the filters, across all pages.</summary>
    public int TotalCount { get; init; }

    /// <summary>Gets the 1-based page number returned.</summary>
    public int PageNumber { get; init; }

    /// <summary>Gets the page size used.</summary>
    public int PageSize { get; init; }
}

/// <summary>Request body for creating a new tenant user account (mirrors <c>CreateUserRequest</c>).</summary>
public sealed class CreateUserRequest
{
    /// <summary>Gets or sets the user's first name. Max 100 characters.</summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>Gets or sets the user's last name. Max 100 characters.</summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>Gets or sets the user's email address (unique within tenant).</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the initial roles to assign. Must contain at least one valid role name.</summary>
    public IReadOnlyList<string> Roles { get; set; } = [];

    /// <summary>Gets or sets the site access list. Empty means no site-specific access.</summary>
    public IReadOnlyList<Guid> SiteIds { get; set; } = [];
}

/// <summary>Response payload returned after creating a user (mirrors <c>CreateUserResponse</c>).</summary>
public sealed class CreateUserResponseDto
{
    /// <summary>Gets the identifier of the newly created user.</summary>
    public Guid UserId { get; init; }
}
