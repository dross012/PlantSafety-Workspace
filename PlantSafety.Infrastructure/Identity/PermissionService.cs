// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Claims-backed implementation of IPermissionService.
//   Evaluates permission grants from the JWT "permission" claims resolved via
//   ICurrentUserService, matching the Entra ID / MSAL app-role-claims pattern.
//   Zero-trust: a user with no matching permission claim is always denied.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.CurrentUser;
using PlantSafety.Application.Abstractions.Security;

namespace PlantSafety.Infrastructure.Identity;

/// <summary>
/// Evaluates permission grants for the authenticated actor by matching against the
/// permission claims resolved by <see cref="ICurrentUserService"/> for the current request.
/// </summary>
public sealed class PermissionService : IPermissionService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<PermissionService> _logger;

    /// <summary>Initialises a new instance of <see cref="PermissionService"/>.</summary>
    /// <param name="currentUserService">The current user identity service.</param>
    /// <param name="logger">The structured logger.</param>
    public PermissionService(ICurrentUserService currentUserService, ILogger<PermissionService> logger)
    {
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task<bool> HasPermissionAsync(Guid userId, Guid tenantId, string permission, CancellationToken cancellationToken = default)
    {
        var granted = MatchesCurrentUser(userId, tenantId) && _currentUserService.HasPermission(permission);

        if (!granted)
        {
            _logger.LogDebug(
                "PermissionCheckDenied | UserId={UserId} | TenantId={TenantId} | Permission={Permission}",
                userId, tenantId, permission);
        }

        return Task.FromResult(granted);
    }

    /// <inheritdoc/>
    public Task<bool> HasAnyPermissionAsync(Guid userId, Guid tenantId, IReadOnlyList<string> permissions, CancellationToken cancellationToken = default)
    {
        var granted = MatchesCurrentUser(userId, tenantId) && permissions.Any(_currentUserService.HasPermission);
        return Task.FromResult(granted);
    }

    /// <inheritdoc/>
    public Task<bool> HasAllPermissionsAsync(Guid userId, Guid tenantId, IReadOnlyList<string> permissions, CancellationToken cancellationToken = default)
    {
        var granted = MatchesCurrentUser(userId, tenantId) && permissions.All(_currentUserService.HasPermission);
        return Task.FromResult(granted);
    }

    /// <summary>
    /// Confirms the permission check target matches the actual authenticated identity for this
    /// request. A mismatch (checking a different user/tenant than the one making the request)
    /// is always denied — this service only evaluates the caller's own claims, it does not look
    /// up other users' permissions.
    /// </summary>
    private bool MatchesCurrentUser(Guid userId, Guid tenantId) =>
        _currentUserService.IsAuthenticated
        && _currentUserService.UserId == userId
        && _currentUserService.TenantId == tenantId;
}
