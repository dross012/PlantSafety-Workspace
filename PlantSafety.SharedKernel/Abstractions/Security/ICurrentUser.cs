/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               ICurrentUser.cs
    DESCRIPTION:        Canonical abstraction representing the authenticated human user in the current context. Used for
                        CRUD authorship fields (CreatedBy, UpdatedBy, DeletedBy) and permission evaluation.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Abstractions.Security;

/// <summary>
/// Represents the authenticated human user in the current context.
/// MUST NOT represent system actors, sensors, or workflows.
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// Strong-typed user identifier (UserId in the domain).
    /// </summary>
    string UserId { get; }

    /// <summary>
    /// Normalized username or login identifier.
    /// </summary>
    string UserName { get; }

    /// <summary>
    /// Returns true if the user has the specified role.
    /// </summary>
    bool IsInRole(string role);

    /// <summary>
    /// Returns true if the user has the specified permission.
    /// </summary>
    bool HasPermission(string permission);
}
