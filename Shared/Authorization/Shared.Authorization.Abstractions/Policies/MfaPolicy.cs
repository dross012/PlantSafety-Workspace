// ============================================================================
//  Project:        Shared.Authorization
//  Module:         Abstractions.Policies
//  Layer:          Abstractions
//  File:           MfaPolicy.cs
//  Created:        2026-08-02
//  Author:         Enterprise Integration & Ecosystem Expansion Agent
//  Description:    Canonical policy name for the enterprise-wide MFA enforcement
//                  requirement (Phase 6, §2.1 "SSO Integration" — MFA
//                  enforcement). Registered by
//                  PlantSafety.Api.Extensions.AuthorizationPolicyExtensions
//                  against a claims-based requirement that checks the standard
//                  OIDC "amr" (Authentication Methods References) claim for the
//                  "mfa" value.
// ============================================================================

namespace Shared.Authorization.Abstractions.Policies;

/// <summary>
/// Canonical authorization policy name for endpoints that require the current
/// session to have completed multi-factor authentication, in addition to
/// whatever Safety-tier or module-level policy already applies. Controllers
/// opt in with <c>[Authorize(Policy = MfaPolicy.RequireMfa)]</c> or the
/// <c>[RequireMfa]</c> attribute shortcut.
/// </summary>
public static class MfaPolicy
{
    /// <summary>
    /// Requires the "amr" (Authentication Methods References, RFC 8176) claim
    /// to contain "mfa", indicating the identity provider enforced a second
    /// authentication factor for this session.
    /// </summary>
    public const string RequireMfa = "RequireMfa";
}
