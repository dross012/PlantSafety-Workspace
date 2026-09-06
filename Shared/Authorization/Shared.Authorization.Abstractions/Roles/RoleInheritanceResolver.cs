// ============================================================================
//  Project:        Shared.Authorization
//  Module:         Abstractions.Roles
//  Layer:          Abstractions
//  File:           RoleInheritanceResolver.cs
//  Created:        2026-08-02
//  Author:         Enterprise Integration & Ecosystem Expansion Agent
//  Description:    Deterministic role-hierarchy resolver (Phase 6, §2.2 "Tenant
//                  Federation" / §2.3 "Role/Claim Standardization"). Given a
//                  role, resolves the full set of roles (and, transitively, the
//                  permissions those roles imply) that role inherits.
// ============================================================================
//
// Inheritance model:
// - The enterprise tier model (see EnterpriseRole) is strictly linear:
//   Viewer < Editor < Approver < PlatformAdmin. Every tier inherits every
//   permission granted to every tier ranked below it — this mirrors the
//   existing "XxxAndAbove" role lists already declared on SafetyRoles
//   (e.g. SafetyRoles.EditorAndAbove = {Editor, Approver, PlatformAdmin}),
//   which describe "which roles satisfy tier X" (the authorization-check
//   direction). RoleInheritanceResolver answers the inverse question —
//   "given a role, which lower tiers does it inherit" — which is what
//   permission-resolution and role-hierarchy display/audit code needs.
// - This is deterministic and side-effect free: no I/O, no configuration,
//   no per-tenant overrides. Per-tenant/module permission overrides are the
//   responsibility of Permissions.IPermissionResolver, which composes this
//   resolver with per-module permission verb sets.
// ============================================================================

namespace Shared.Authorization.Abstractions.Roles;

/// <summary>
/// Resolves the full, deterministic role-inheritance closure for any
/// <see cref="EnterpriseRole"/>: every role ranked at or below the given role
/// is inherited by it.
/// </summary>
public static class RoleInheritanceResolver
{
    /// <summary>
    /// Resolves every <see cref="EnterpriseRole"/> that <paramref name="role"/> inherits,
    /// including itself. The result is ordered from lowest to highest rank.
    /// </summary>
    /// <param name="role">The role whose inheritance closure is resolved.</param>
    /// <returns>
    /// An ordered, deterministic set of roles at or below <paramref name="role"/>'s rank.
    /// </returns>
    public static IReadOnlyList<EnterpriseRole> GetInheritedRoles(EnterpriseRole role) =>
        EnterpriseRole.All.Where(r => r.Rank <= role.Rank).OrderBy(r => r.Rank).ToArray();

    /// <summary>
    /// Resolves every <see cref="EnterpriseRole"/> that <paramref name="role"/> inherits,
    /// expressed as their canonical role-claim name strings (see <see cref="SafetyRoles"/>).
    /// </summary>
    /// <param name="role">The role whose inheritance closure is resolved.</param>
    public static IReadOnlySet<string> GetInheritedRoleNames(EnterpriseRole role) =>
        GetInheritedRoles(role).Select(r => r.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="candidate"/> is satisfied by
    /// (i.e. is inherited by) <paramref name="held"/> — the standard "does the held role
    /// meet or exceed the required tier" authorization check, expressed in terms of the
    /// inheritance model rather than a hard-coded "AndAbove" list.
    /// </summary>
    /// <param name="held">The role the caller actually holds.</param>
    /// <param name="candidate">The minimum role required for the operation.</param>
    public static bool Satisfies(EnterpriseRole held, EnterpriseRole candidate) =>
        held.Rank >= candidate.Rank;

    /// <summary>
    /// Resolves the highest-ranked <see cref="EnterpriseRole"/> present in a set of raw
    /// role-claim strings (e.g. from a JWT "role" claim collection), or <see langword="null"/>
    /// when none of the supplied claims match a canonical enterprise role.
    /// </summary>
    /// <param name="roleClaimValues">The raw role-claim strings held by the current principal.</param>
    public static EnterpriseRole? ResolveHighestRole(IEnumerable<string> roleClaimValues) =>
        roleClaimValues
            .Select(EnterpriseRole.FromClaimValue)
            .Where(r => r is not null)
            .OrderByDescending(r => r!.Rank)
            .FirstOrDefault();
}
