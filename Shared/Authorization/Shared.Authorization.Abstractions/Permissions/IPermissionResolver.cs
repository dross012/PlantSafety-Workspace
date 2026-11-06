// ============================================================================
//  Project:        Shared.Authorization
//  Module:         Abstractions.Permissions
//  Layer:          Abstractions
//  File:           IPermissionResolver.cs
//  Created:        2026-08-02
//  Author:         Enterprise Integration & Ecosystem Expansion Agent
//  Description:    Module-level permission resolution abstraction (Phase 6,
//                  §2.3 "Role/Claim Standardization" — module-level permissions
//                  + cross-module permission inheritance). Given a role and a
//                  module name, resolves the effective, module-qualified
//                  permission set for that role, respecting the enterprise
//                  role-inheritance rules (Roles.RoleInheritanceResolver).
// ============================================================================

using Shared.Authorization.Abstractions.Roles;

namespace Shared.Authorization.Abstractions.Permissions;

/// <summary>
/// Resolves the effective, module-qualified permission set granted to a role
/// within a given module, including every permission inherited from lower
/// enterprise tiers (see <see cref="RoleInheritanceResolver"/>).
/// </summary>
public interface IPermissionResolver
{
    /// <summary>
    /// Resolves every module-qualified permission string (e.g. <c>"Analytics.Read"</c>,
    /// <c>"Analytics.Write"</c>) granted to <paramref name="role"/> within
    /// <paramref name="moduleName"/>, including permissions inherited from every
    /// lower-ranked enterprise tier.
    /// </summary>
    /// <param name="role">The role to resolve permissions for.</param>
    /// <param name="moduleName">
    /// The bounded-context/module name the permission is scoped to (e.g. <c>"Analytics"</c>,
    /// <c>"SafetyEvents"</c>). Matches the <c>&lt;Module&gt;</c> segment already used by the
    /// ~140 per-module <c>*AuthorizationPolicies</c> policy-name constants.
    /// </param>
    /// <returns>
    /// A deterministic, ordered set of module-qualified permission strings. Never
    /// <see langword="null"/>; an unrecognised role or module resolves to an empty set
    /// (zero-trust default-deny), never throws.
    /// </returns>
    IReadOnlySet<string> Resolve(EnterpriseRole role, string moduleName);

    /// <summary>
    /// Resolves every module-qualified permission string granted across
    /// <paramref name="moduleNames"/> for <paramref name="role"/> — the cross-module
    /// permission inheritance case (a role's tier applies identically in every module
    /// it is scoped to; no module grants a role more than its tier allows).
    /// </summary>
    /// <param name="role">The role to resolve permissions for.</param>
    /// <param name="moduleNames">Every module name to resolve permissions within.</param>
    IReadOnlySet<string> ResolveAcrossModules(EnterpriseRole role, IEnumerable<string> moduleNames);
}
