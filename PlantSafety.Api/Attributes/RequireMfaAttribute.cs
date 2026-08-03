// ============================================================================
// File: RequireMfaAttribute.cs
// Project: PlantSafety.Api
// Author: Enterprise Integration & Ecosystem Expansion Agent
// Created: 2026-08-02
// Description: Convenience [Authorize] shortcut enforcing the enterprise-wide
//              MFA policy (Phase 6 §2.1 "SSO Integration" — MFA enforcement).
//              Equivalent to [Authorize(Policy = MfaPolicy.RequireMfa)], but
//              expresses intent directly at the call site.
// ============================================================================
//
// Onboarding Notes:
// - Apply to any controller or action that requires a second authentication
//   factor beyond standard bearer-token authentication, e.g. destructive
//   admin operations, credential/secret rotation endpoints, or any endpoint
//   your compliance policy designates as step-up-auth-required.
// - This is ADDITIVE: stack it alongside the existing Safety-tier policy —
//   ASP.NET Core evaluates every [Authorize] attribute on an action as an
//   AND (all must pass), so [RequireMfa] never weakens an existing
//   [Authorize(Policy = SafetyTierPolicies.Admin)] etc.
// - The underlying policy is registered by
//   PlantSafety.Api.Extensions.AuthorizationPolicyExtensions.AddPlantSafetyPolicies()
//   and checks the standard OIDC "amr" claim for the value "mfa".
//
// ============================================================================

using Microsoft.AspNetCore.Authorization;
using Shared.Authorization.Abstractions.Policies;

namespace PlantSafety.Api.Attributes;

/// <summary>
/// Requires the current session's "amr" claim to indicate multi-factor
/// authentication was completed, in addition to any other authorization
/// policy already applied to the decorated controller or action.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class RequireMfaAttribute : AuthorizeAttribute
{
    /// <summary>Initialises a new instance of <see cref="RequireMfaAttribute"/>.</summary>
    public RequireMfaAttribute() : base(MfaPolicy.RequireMfa)
    {
    }
}
