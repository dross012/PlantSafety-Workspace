// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Permission and authorization check abstraction for Application layer.
//   Authorization is enforced at the API boundary and re-validated in the pipeline behavior.
//   MUST enforce zero-trust: every operation requires an explicit permission check.
// SchemaVersion: 1
// Additive-only evolution: new members may be added; no members may be removed or renamed.

namespace PlantSafety.Application.Abstractions.Security;

/// <summary>
/// Evaluates authorization policies and permission checks for the authenticated actor.
/// Used by <c>AuthorizationBehavior</c> to enforce zero-trust at the Application layer.
/// Infrastructure provides the Entra ID / MSAL-backed implementation.
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// Returns <c>true</c> if the specified user holds the named permission within the given tenant.
    /// </summary>
    /// <param name="userId">The user identifier to check.</param>
    /// <param name="tenantId">The tenant scope for the permission check.</param>
    /// <param name="permission">The fully-qualified permission name (e.g., <c>SafetyEvents.Incidents.Create</c>).</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns><c>true</c> if permission is granted; <c>false</c> otherwise.</returns>
    Task<bool> HasPermissionAsync(Guid userId, Guid tenantId, string permission, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns <c>true</c> if the specified user holds any of the named permissions within the given tenant.
    /// </summary>
    /// <param name="userId">The user identifier to check.</param>
    /// <param name="tenantId">The tenant scope for the permission check.</param>
    /// <param name="permissions">The set of permission names to evaluate (OR logic — any one grants access).</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task<bool> HasAnyPermissionAsync(Guid userId, Guid tenantId, IReadOnlyList<string> permissions, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns <c>true</c> if the specified user holds all of the named permissions within the given tenant.
    /// </summary>
    /// <param name="userId">The user identifier to check.</param>
    /// <param name="tenantId">The tenant scope for the permission check.</param>
    /// <param name="permissions">The set of permission names to evaluate (AND logic — all must be held).</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task<bool> HasAllPermissionsAsync(Guid userId, Guid tenantId, IReadOnlyList<string> permissions, CancellationToken cancellationToken = default);
}
