// ============================================================================
//  Project:        Shared.Authorization
//  Module:         Abstractions.Roles
//  Layer:          Abstractions
//  File:           EnterpriseRole.cs
//  Created:        2026-08-02
//  Author:         Enterprise Integration & Ecosystem Expansion Agent
//  Description:    Canonical, enterprise-wide role taxonomy registry (Phase 6,
//                  §2.3 "Role/Claim Standardization"). Formalizes the same
//                  4-tier model already established by <see cref="SafetyRoles"/>
//                  and <see cref="Policies.SafetyTierPolicies"/> (Viewer / Editor /
//                  Approver / PlatformAdmin) as a single, module-agnostic,
//                  smart-enum-style registry that every bounded context and every
//                  federated partner/tenant mapping resolves against, instead of
//                  re-deriving the tier list ad hoc.
// ============================================================================
//
// Provenance:
// - This type intentionally does NOT introduce a new taxonomy. SafetyRoles.cs
//   (2026-07-27) already reconciled the two conventions found in the codebase
//   (2-tier Read/Write per-module policies, and the Approve-as-distinct-verb
//   convention) into a 4-tier model. EnterpriseRole promotes that same 4-tier
//   model to a first-class, ordered value type (smart enum) so that:
//     1. Role-inheritance can be computed deterministically by rank
//        (see RoleInheritanceResolver).
//     2. Cross-module permission resolution (see Permissions.IPermissionResolver)
//        has a single canonical rank ordering to reason about, instead of
//        string-comparing role names.
//     3. Tenant-federation code (external IdP role/claim mapping, partner
//        identity mapping) has one canonical type to map external claims onto.
//
// ============================================================================

namespace Shared.Authorization.Abstractions.Roles;

/// <summary>
/// Canonical, enterprise-wide role taxonomy entry. A minimal smart-enum over the
/// same four tiers already defined as string constants on <see cref="SafetyRoles"/>
/// (kept for backward compatibility with the ~140 existing policy classes that
/// reference those string constants directly). New cross-cutting code — role
/// inheritance resolution, permission resolution, external IdP role mapping —
/// should use <see cref="EnterpriseRole"/> instead of raw role-name strings.
/// </summary>
public sealed class EnterpriseRole : IEquatable<EnterpriseRole>
{
    /// <summary>Read-only access to Safety-domain data. Lowest tier. Rank 0.</summary>
    public static readonly EnterpriseRole Viewer = new(SafetyRoles.Viewer, rank: 0);

    /// <summary>Create/update access to Safety-domain records. Rank 1.</summary>
    public static readonly EnterpriseRole Editor = new(SafetyRoles.Editor, rank: 1);

    /// <summary>Elevated write access for approve/close/escalate/dispatch/resolve/reject/cancel operations. Rank 2.</summary>
    public static readonly EnterpriseRole Approver = new(SafetyRoles.Approver, rank: 2);

    /// <summary>Full administrative override — the highest tier. Rank 3.</summary>
    public static readonly EnterpriseRole PlatformAdmin = new(SafetyRoles.PlatformAdmin, rank: 3);

    /// <summary>All canonical enterprise roles, ordered from lowest to highest rank.</summary>
    public static readonly IReadOnlyList<EnterpriseRole> All = new[] { Viewer, Editor, Approver, PlatformAdmin };

    private EnterpriseRole(string name, int rank)
    {
        Name = name;
        Rank = rank;
    }

    /// <summary>Gets the canonical role-claim string (matches the corresponding <see cref="SafetyRoles"/> constant).</summary>
    public string Name { get; }

    /// <summary>
    /// Gets the ordinal rank used for deterministic inheritance computation. Higher rank
    /// inherits every permission granted to every lower rank (see <see cref="RoleInheritanceResolver"/>).
    /// </summary>
    public int Rank { get; }

    /// <summary>
    /// Resolves the <see cref="EnterpriseRole"/> matching the supplied role-claim string,
    /// or <see langword="null"/> when the string does not match any canonical role
    /// (e.g. a legacy or module-specific role name not part of the enterprise tier model).
    /// </summary>
    /// <param name="roleClaimValue">The raw role-claim string, e.g. from a JWT "role" claim.</param>
    public static EnterpriseRole? FromClaimValue(string? roleClaimValue) =>
        All.FirstOrDefault(r => string.Equals(r.Name, roleClaimValue, StringComparison.OrdinalIgnoreCase));

    /// <inheritdoc />
    public bool Equals(EnterpriseRole? other) => other is not null && Rank == other.Rank;

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as EnterpriseRole);

    /// <inheritdoc />
    public override int GetHashCode() => Rank;

    /// <inheritdoc />
    public override string ToString() => Name;
}
