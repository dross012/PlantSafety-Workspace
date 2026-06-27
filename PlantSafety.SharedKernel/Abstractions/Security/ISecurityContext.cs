/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               ISecurityContext.cs
    DESCRIPTION:        Canonical security context abstraction. Represents the authenticated actor, their identities,
                        permissions, and correlation metadata for the current execution scope.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Abstractions.Security;

/// <summary>
/// Canonical security context for the current execution scope.
/// MUST distinguish CRUD identity (UserId) from domain action identity (PerformedByActor),
/// as required by the Actor & User Identity Semantics Rule.
/// </summary>
public interface ISecurityContext
{
    /// <summary>
    /// Returns the current authenticated user, if any.
    /// </summary>
    ICurrentUser? CurrentUser { get; }

    /// <summary>
    /// Returns true if there is an authenticated user in the current context.
    /// </summary>
    bool IsAuthenticated { get; }
}
