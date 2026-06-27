// ============================================================================
// File: AuthorizationPolicyExtensions.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: Extension methods that enumerate and register all module-level
//              authorization policy names into the ASP.NET Core authorization
//              system. Delegates to the per-module *AuthorizationPolicies classes
//              for the canonical policy name strings.
// ============================================================================
//
// Onboarding Notes:
// - Called from ServiceCollectionExtensions.AddPlantSafetyApi() after the base
//   RequireAuthenticatedUser and RequireDashboardRead policies are registered.
// - Each policy here maps to a matching role claim or permission scope that the
//   Identity / Azure AD tenant configuration must publish in the JWT.
// - The policy builder pattern used here (RequireRole) is a placeholder. For
//   production, these should be replaced with RequireClaim("permission", policyName)
//   or a custom IAuthorizationRequirement backed by the Roles bounded context.
// - Adding a new module? Add its *AuthorizationPolicies.Read / Create / etc.
//   constants to the RegisterModulePolicies call below. No other file needs changing.
//
// Rationale:
// - Without central registration, each *AuthorizationPolicies class is just
//   constants. This extension is what wires them to the runtime authorization
//   pipeline so [Authorize(Policy = ...)] attributes evaluate correctly.
//
// Future Extension Seams:
// - Replace RequireRole with a fine-grained permission claim evaluator once the
//   Roles bounded context publishes per-resource permission tokens in the JWT.
// - Add policy expiry or version-gating once API v3 introduces breaking permission
//   splits.
//
// ============================================================================

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
        // ── Analytics ────────────────────────────────────────────────────────
        options.AddPolicy("Analytics.Read",   RequireRole("Analytics.Read",   "Platform.Admin"));
        options.AddPolicy("Analytics.Export", RequireRole("Analytics.Export", "Platform.Admin"));

        // ── Compliance ───────────────────────────────────────────────────────
        options.AddPolicy("Compliance.Read",    RequireRole("Compliance.Read",    "Platform.Admin"));
        options.AddPolicy("Compliance.Create",  RequireRole("Compliance.Create",  "Platform.Admin"));
        options.AddPolicy("Compliance.Update",  RequireRole("Compliance.Update",  "Platform.Admin"));
        options.AddPolicy("Compliance.Approve", RequireRole("Compliance.Approve", "Platform.Admin"));
        options.AddPolicy("Compliance.Delete",  RequireRole("Compliance.Delete",  "Platform.Admin"));

        options.AddPolicy("Compliance.Export",  RequireRole("Compliance.Export",  "Platform.Admin"));

        // ── ContinuousImprovement ────────────────────────────────────────────
        options.AddPolicy("ContinuousImprovement.Read",    RequireRole("ContinuousImprovement.Read",    "Platform.Admin"));
        options.AddPolicy("ContinuousImprovement.Create",  RequireRole("ContinuousImprovement.Create",  "Platform.Admin"));
        options.AddPolicy("ContinuousImprovement.Update",  RequireRole("ContinuousImprovement.Update",  "Platform.Admin"));
        options.AddPolicy("ContinuousImprovement.Approve", RequireRole("ContinuousImprovement.Approve", "Platform.Admin"));
        options.AddPolicy("ContinuousImprovement.Delete",  RequireRole("ContinuousImprovement.Delete",  "Platform.Admin"));
        options.AddPolicy("ContinuousImprovement.Export",  RequireRole("ContinuousImprovement.Export",  "Platform.Admin"));

        // ── Integrations ─────────────────────────────────────────────────────
        options.AddPolicy("Integrations.Read",   RequireRole("Integrations.Read",   "Platform.Admin"));
        options.AddPolicy("Integrations.Create", RequireRole("Integrations.Create", "Platform.Admin"));
        options.AddPolicy("Integrations.Update", RequireRole("Integrations.Update", "Platform.Admin"));
        options.AddPolicy("Integrations.Delete", RequireRole("Integrations.Delete", "Platform.Admin"));

        // ── Intelligence ──────────────────────────────────────────────────────
        options.AddPolicy("Intelligence.Read",   RequireRole("Intelligence.Read",   "Platform.Admin"));
        options.AddPolicy("Intelligence.Export", RequireRole("Intelligence.Export", "Platform.Admin"));

        // ── Operational ──────────────────────────────────────────────────────
        options.AddPolicy("Operational.Read",    RequireRole("Operational.Read",    "Platform.Admin"));
        options.AddPolicy("Operational.Create",  RequireRole("Operational.Create",  "Platform.Admin"));
        options.AddPolicy("Operational.Update",  RequireRole("Operational.Update",  "Platform.Admin"));
        options.AddPolicy("Operational.Approve", RequireRole("Operational.Approve", "Platform.Admin"));
        options.AddPolicy("Operational.Delete",  RequireRole("Operational.Delete",  "Platform.Admin"));
        options.AddPolicy("Operational.Export",  RequireRole("Operational.Export",  "Platform.Admin"));

        // ── SafetyEvents ─────────────────────────────────────────────────────
        options.AddPolicy("SafetyEvents.Read",    RequireRole("SafetyEvents.Read",    "Platform.Admin"));
        options.AddPolicy("SafetyEvents.Create",  RequireRole("SafetyEvents.Create",  "Platform.Admin"));
        options.AddPolicy("SafetyEvents.Update",  RequireRole("SafetyEvents.Update",  "Platform.Admin"));
        options.AddPolicy("SafetyEvents.Approve", RequireRole("SafetyEvents.Approve", "Platform.Admin"));
        options.AddPolicy("SafetyEvents.Delete",  RequireRole("SafetyEvents.Delete",  "Platform.Admin"));
        options.AddPolicy("SafetyEvents.Export",  RequireRole("SafetyEvents.Export",  "Platform.Admin"));

        // ── Webhooks ──────────────────────────────────────────────────────────
        options.AddPolicy("Webhooks.Manage",   RequireRole("Webhooks.Manage",   "Platform.Admin"));
        options.AddPolicy("Webhooks.ViewLogs", RequireRole("Webhooks.ViewLogs", "Platform.Admin"));
    }

    private static AuthorizationPolicy RequireRole(params string[] roles) =>
        new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .RequireRole(roles)
            .Build();
}
