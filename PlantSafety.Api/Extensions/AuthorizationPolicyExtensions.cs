// ============================================================================
// File: AuthorizationPolicyExtensions.cs
// Project: PlantSafety.Api
// Author: David (Ted); rewritten by the Security & Compliance Agent 2026-07-27.
// Created: 2026-06-13
// Last Updated: 2026-07-27
// Description: Extension methods that enumerate and register EVERY module-level
//              authorization policy name into the ASP.NET Core authorization
//              system, plus the four canonical Safety-tier policies
//              (Shared.Authorization.Abstractions.Policies.SafetyTierPolicies).
// ============================================================================
//
// SECURITY FINDING FIXED HERE (see docs/architecture/authorization-tiers.md):
// - AddPlantSafetyPolicies() existed but was NEVER CALLED from
//   ServiceCollectionExtensions.AddPlantSafetyAuthorization() — confirmed by
//   grepping the whole PlantSafety.Api tree for its only reference (its own
//   declaration). It also only registered ~34 coarse "<Domain>.<Verb>" policy
//   names (e.g. "Analytics.Read"), which NONE of the ~140 per-bounded-context
//   *AuthorizationPolicies classes' constants (e.g.
//   IncidentManagementAuthorizationPolicies.Read =
//   "RequireSafetyEventsIncidentManagementRead") ever referenced. The practical
//   effect: every one of the ~300+ controller actions using
//   [Authorize(Policy = "Require<Module><Verb>")] pointed at a policy name that
//   was registered NOWHERE in the authorization system, which ASP.NET Core's
//   AuthorizationMiddleware treats as a hard failure (InvalidOperationException)
//   the first time that action is invoked.
// - Fix: this method now reflects over every `*AuthorizationPolicies` class in
//   the PlantSafety.Api assembly (the same convention already used for their
//   folder/file naming), reads every public const string field it declares, and
//   registers a matching AuthorizationOptions policy for each — inferring the
//   required Safety-tier from the constant's own name (Read → ViewerAndAbove,
//   Create/Update/Write → EditorAndAbove, Approve → ApproverAndAbove,
//   Delete/Manage → AdminOnly, Export/ViewLogs → ApproverAndAbove). New modules
//   therefore get correctly-registered policies automatically, with no drift
//   risk from a second, manually-maintained list going stale (which is exactly
//   what happened here).
// - The four canonical Safety.Tier.* policies are also registered here so the
//   ~57 legacy controllers that previously enforced only "RequireAuthenticatedUser"
//   (see docs/architecture/authorization-tiers.md for the full list) can enforce
//   correctly-separated read/write/approve tiers via
//   Shared.Authorization.Abstractions.Policies.SafetyTierPolicies.
//
// Onboarding Notes:
// - Called from ServiceCollectionExtensions.AddPlantSafetyAuthorization().
// - Each policy here maps to a matching role claim that the Identity / Azure AD
//   tenant configuration must publish in the JWT. RequireRole is a placeholder;
//   production should move to RequireClaim("permission", policyName) or a custom
//   IAuthorizationRequirement backed by the Roles bounded context (unchanged from
//   the original design intent).
// - Adding a new module? Add its `*AuthorizationPolicies` class next to its
//   controllers (as already done for the existing ~140) — no other file needs
//   changing; this method discovers it automatically.
//
// ============================================================================

using System.Reflection;
using Shared.Authorization.Abstractions.Policies;
using Shared.Authorization.Abstractions.Roles;

namespace PlantSafety.Api.Extensions;

/// <summary>
/// Extension methods for registering all PlantSafety.Api module authorization policies.
/// </summary>
public static class AuthorizationPolicyExtensions
{
    /// <summary>
    /// Adds all module-level authorization policies to the <see cref="AuthorizationOptions"/>.
    /// Call this from within <c>services.AddAuthorization(options => { ... })</c>.
    /// </summary>
    /// <param name="options">The authorization options to configure.</param>
    public static void AddPlantSafetyPolicies(this AuthorizationOptions options)
    {
        // ── Canonical Safety tiers (used directly by legacy controllers that
        //    previously only enforced RequireAuthenticatedUser) ──────────────
        options.AddPolicy(SafetyTierPolicies.Read,    RequireRoles(SafetyRoles.ViewerAndAbove));
        options.AddPolicy(SafetyTierPolicies.Write,   RequireRoles(SafetyRoles.EditorAndAbove));
        options.AddPolicy(SafetyTierPolicies.Approve, RequireRoles(SafetyRoles.ApproverAndAbove));
        options.AddPolicy(SafetyTierPolicies.Admin,   RequireRoles(SafetyRoles.AdminOnly));

        // ── MFA enforcement (Phase 6 §2.1) ──────────────────────────────────
        // Checks the standard OIDC "amr" (Authentication Methods References,
        // RFC 8176) claim for a "mfa" value. Any controller opts in with
        // [Authorize(Policy = MfaPolicy.RequireMfa)] or the [RequireMfa]
        // attribute shortcut (PlantSafety.Api.Attributes.RequireMfaAttribute).
        // This is purely additive: it does not replace or weaken any existing
        // role/tier policy, it is layered alongside them.
        options.AddPolicy(MfaPolicy.RequireMfa, policy => policy
            .RequireAuthenticatedUser()
            .RequireAssertion(context => context.User.Claims.Any(c =>
                string.Equals(c.Type, "amr", StringComparison.OrdinalIgnoreCase)
                && string.Equals(c.Value, "mfa", StringComparison.OrdinalIgnoreCase))));

        // ── Every per-module *AuthorizationPolicies class's policy-name
        //    constants, auto-discovered so new modules can never drift out of
        //    registration again ───────────────────────────────────────────────
        foreach (var (policyName, tier) in DiscoverModulePolicies())
        {
            options.AddPolicy(policyName, RequireRoles(RolesForTier(tier)));
        }
    }

    /// <summary>
    /// Reflects over the PlantSafety.Api assembly for every class whose name ends
    /// in "AuthorizationPolicies" (the established per-module naming convention)
    /// and yields each public const string field's value paired with the tier
    /// inferred from the field's own name.
    /// </summary>
    private static IEnumerable<(string PolicyName, string Tier)> DiscoverModulePolicies()
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);

        var policyClasses = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsClass && t.IsAbstract && t.IsSealed // static class
                        && t.Name.EndsWith("AuthorizationPolicies", StringComparison.Ordinal));

        foreach (var type in policyClasses)
        {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string));

            foreach (var field in fields)
            {
                var policyName = (string)field.GetRawConstantValue()!;
                if (!seen.Add(policyName))
                    continue; // some legacy modules re-declare identical constants across V1/V2; register once.

                yield return (policyName, TierForFieldName(field.Name));
            }
        }
    }

    /// <summary>
    /// Infers the required Safety tier from a policy constant's field name
    /// (e.g. "Read", "Write", "Create", "Update", "Approve", "Delete", "Export",
    /// "Manage", "ViewLogs"). Defaults to the most restrictive tier (Admin) for
    /// any verb not recognised, per the "on ambiguity, choose the stricter
    /// option" rule.
    /// </summary>
    private static string TierForFieldName(string fieldName) => fieldName switch
    {
        "Read" => "Read",
        "Write" or "Create" or "Update" => "Write",
        "Approve" or "Export" or "ViewLogs" => "Approve",
        "Delete" or "Manage" => "Admin",
        _ => "Admin",
    };

    private static IReadOnlyList<string> RolesForTier(string tier) => tier switch
    {
        "Read" => SafetyRoles.ViewerAndAbove,
        "Write" => SafetyRoles.EditorAndAbove,
        "Approve" => SafetyRoles.ApproverAndAbove,
        _ => SafetyRoles.AdminOnly,
    };

    private static AuthorizationPolicy RequireRoles(IReadOnlyList<string> roles) =>
        new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .RequireRole(roles)
            .Build();
}
