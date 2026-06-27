// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Current user and actor identity abstraction.
//   This interface enforces the Actor & User Identity Semantics Rule (§1.8):
//   CRUD identity (UserId) is separate from Domain Action identity (ActorId).
//   UserId tracks CreatedBy/UpdatedBy; ActorId covers all domain actions.
// SchemaVersion: 1
// Additive-only evolution: new members may be added; no members may be removed or renamed.

namespace PlantSafety.Application.Abstractions.CurrentUser;

/// <summary>
/// Provides the authenticated identity of the current user and the domain action actor.
/// Enforces the Actor &amp; User Identity Semantics Rule (§1.8):
/// <c>UserId</c> is used exclusively for CRUD audit fields (CreatedBy, UpdatedBy);
/// <c>ActorId</c> is used for all domain actions (AcknowledgedBy, EscalatedBy, etc.).
/// </summary>
public interface ICurrentUserService
{
    /// <summary>Gets the authenticated user identifier for CRUD audit fields. Never null once authenticated.</summary>
    Guid UserId { get; }

    /// <summary>Gets the domain actor identifier for all domain action tracking. May represent User, System, Workflow, Sensor, or External actor.</summary>
    Guid ActorId { get; }

    /// <summary>Gets the actor type for structured logging and domain event attribution.</summary>
    ActorType ActorType { get; }

    /// <summary>Gets the tenant identifier scoped to the authenticated user's session.</summary>
    Guid TenantId { get; }

    /// <summary>Gets the site identifier if the user is scoped to a specific site; null for platform-level operations.</summary>
    Guid? SiteId { get; }

    /// <summary>Gets the roles assigned to the authenticated user.</summary>
    IReadOnlyList<string> Roles { get; }

    /// <summary>Gets the permissions granted to the authenticated user.</summary>
    IReadOnlyList<string> Permissions { get; }

    /// <summary>Returns true if the current user holds the specified permission.</summary>
    /// <param name="permission">The permission string to check.</param>
    /// <returns><c>true</c> if the user holds the permission; otherwise <c>false</c>.</returns>
    bool HasPermission(string permission);

    /// <summary>Returns true if the current user holds the specified role.</summary>
    /// <param name="role">The role name to check.</param>
    /// <returns><c>true</c> if the user holds the role; otherwise <c>false</c>.</returns>
    bool IsInRole(string role);

    /// <summary>Returns true when a valid, authenticated identity is present in the current execution context.</summary>
    bool IsAuthenticated { get; }
}

/// <summary>
/// Canonical actor type classification for domain event attribution and structured logging.
/// </summary>
public enum ActorType
{
    /// <summary>Action performed by a human user via the UI or API.</summary>
    User = 0,

    /// <summary>Action performed by an automated system process or background job.</summary>
    System = 1,

    /// <summary>Action performed by a workflow engine or saga orchestrator.</summary>
    Workflow = 2,

    /// <summary>Action reported by an IoT sensor or telemetry device.</summary>
    Sensor = 3,

    /// <summary>Action performed by an external integrated system.</summary>
    External = 4
}
