// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: HttpContext/JWT-claims-backed implementation of ICurrentUserService.
//   Resolves the authenticated user's identity, actor identity, tenant, and roles/permissions
//   from the current request's ClaimsPrincipal (populated by JWT bearer authentication).
// SchemaVersion: 1

using Microsoft.AspNetCore.Http;
using PlantSafety.Application.Abstractions.CurrentUser;

namespace PlantSafety.Infrastructure.Identity;

/// <summary>
/// Resolves the current authenticated user's identity from the request's JWT claims.
/// Enforces the Actor &amp; User Identity Semantics Rule (§1.8): <see cref="UserId"/> is the
/// CRUD-audit identity; <see cref="ActorId"/> is the domain-action identity (defaults to
/// <see cref="UserId"/> for human-initiated requests, since a human actor's UserId and ActorId
/// are the same identifier — only System/Workflow/Sensor/External actors diverge, and those
/// paths do not originate from an authenticated HTTP request).
/// </summary>
public sealed class CurrentUserService : ICurrentUserService
{
    private const string UserIdClaimType = "sub";
    private const string TenantIdClaimType = "tenant_id";
    private const string SiteIdClaimType = "site_id";
    private const string RoleClaimType = "role";
    private const string PermissionClaimType = "permission";

    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>Initialises a new instance of <see cref="CurrentUserService"/>.</summary>
    /// <param name="httpContextAccessor">ASP.NET Core HTTP context accessor.</param>
    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private System.Security.Claims.ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    /// <inheritdoc/>
    public Guid UserId
    {
        get
        {
            var claim = User?.FindFirst(UserIdClaimType)?.Value
                ?? User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
        }
    }

    /// <inheritdoc/>
    public Guid ActorId => UserId;

    /// <inheritdoc/>
    public ActorType ActorType => ActorType.User;

    /// <inheritdoc/>
    public Guid TenantId
    {
        get
        {
            var claim = User?.FindFirst(TenantIdClaimType)?.Value;
            return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
        }
    }

    /// <inheritdoc/>
    public Guid? SiteId
    {
        get
        {
            var claim = User?.FindFirst(SiteIdClaimType)?.Value;
            return Guid.TryParse(claim, out var id) ? id : null;
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<string> Roles =>
        User?.FindAll(RoleClaimType).Select(c => c.Value).ToList()
        ?? User?.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList()
        ?? [];

    /// <inheritdoc/>
    public IReadOnlyList<string> Permissions =>
        User?.FindAll(PermissionClaimType).Select(c => c.Value).ToList() ?? [];

    /// <inheritdoc/>
    public bool HasPermission(string permission) =>
        Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public bool IsInRole(string role) =>
        User?.IsInRole(role) ?? Roles.Contains(role, StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
}
