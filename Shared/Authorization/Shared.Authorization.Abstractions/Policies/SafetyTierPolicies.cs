// ============================================================================
//  Project:        Shared.Authorization
//  Module:         Abstractions.Policies
//  Layer:          Abstractions
//  File:           SafetyTierPolicies.cs
//  Created:        2026-07-27
//  Author:         Security & Compliance Agent
//  Description:    Canonical, tier-based ASP.NET Core authorization policy name
//                  constants for the Safety domain. These are the four policies
//                  every legacy controller that previously enforced only
//                  "RequireAuthenticatedUser" (no role distinction between reads
//                  and writes) now maps its actions onto — see
//                  docs/architecture/authorization-tiers.md for the full
//                  controller-to-tier mapping table.
//
//  Naming note: deliberately NOT named "SafetyAuthorizationPolicies" — that name
//  collides with the pre-existing per-module
//  PlantSafety.Api.Compliance.Procedures.Safety.Authorization.SafetyAuthorizationPolicies
//  class (the "Safety" bounded-context module's own Read/Write policy names),
//  which would be an ambiguous-reference compile error once both are in scope via
//  PlantSafety.Api's global usings.
// ============================================================================

namespace Shared.Authorization.Abstractions.Policies;

/// <summary>
/// Canonical tier-based authorization policy names, registered by
/// PlantSafety.Api.Extensions.AuthorizationPolicyExtensions.AddPlantSafetyPolicies
/// against the matching <see cref="Roles.SafetyRoles"/> role sets.
/// </summary>
public static class SafetyTierPolicies
{
    /// <summary>Requires <see cref="Roles.SafetyRoles.ViewerAndAbove"/>. Use on GET endpoints.</summary>
    public const string Read = "Safety.Tier.Read";

    /// <summary>Requires <see cref="Roles.SafetyRoles.EditorAndAbove"/>. Use on standard create/update endpoints.</summary>
    public const string Write = "Safety.Tier.Write";

    /// <summary>Requires <see cref="Roles.SafetyRoles.ApproverAndAbove"/>. Use on approve/close/escalate/dispatch/resolve endpoints.</summary>
    public const string Approve = "Safety.Tier.Approve";

    /// <summary>Requires <see cref="Roles.SafetyRoles.AdminOnly"/>. Use on destructive/admin/backup/restore endpoints.</summary>
    public const string Admin = "Safety.Tier.Admin";
}
