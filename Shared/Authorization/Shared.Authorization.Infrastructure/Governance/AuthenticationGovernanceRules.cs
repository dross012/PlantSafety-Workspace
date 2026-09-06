// ============================================================================
//  Project:        Shared.Authorization.Infrastructure
//  Module:         Governance
//  File:           AuthenticationGovernanceRules.cs
//  Created:        2026-08-10
//  Description:    Phase 10 deliverable. Pure validation of a
//                  CallerContextResolution's shape -- mirrors
//                  AuthorizationGovernanceRules (Phase 9) exactly: static
//                  methods, no I/O, no side effects, returns a list of
//                  violation-name strings (empty when valid). Validates only
//                  what this phase actually produces (API-key resolution
//                  outcomes and the CallerContext derived from them); does
//                  not duplicate AuthorizationGovernanceRules' own
//                  CallerContext-shape checks, which still run wherever
//                  AuthorizationGovernanceEvaluator already runs.
// ============================================================================
using Shared.Authorization.Abstractions.Caller;

namespace Shared.Authorization.Infrastructure.Governance;

/// <summary>Pure validation rules for a CallerContextResolution produced by the authentication layer.</summary>
public static class AuthenticationGovernanceRules
{
    /// <summary>
    /// Validates <paramref name="resolution"/>. Reports "MissingResolution" when null;
    /// "UnrecognizedKeyMetadataMissing" when a key was provided but not recognized and the
    /// resolved role/correlation id are missing; "InvalidCallerContextShape" when the resolved
    /// CallerContext itself is malformed (empty correlation id or null role).
    /// </summary>
    public static IReadOnlyList<string> ValidateResolution(CallerContextResolution? resolution)
    {
        var violations = new List<string>();

        if (resolution is null)
        {
            violations.Add("MissingResolution");
            return violations;
        }

        if (resolution.Caller is null)
        {
            violations.Add("MissingCallerContext");
            return violations;
        }

        if (string.IsNullOrWhiteSpace(resolution.Caller.CorrelationId))
        {
            violations.Add("InvalidCallerContextShape");
        }

        if (resolution.Identity.KeyProvided && !resolution.Identity.KeyRecognized && resolution.Identity.ResolvedRole is null)
        {
            violations.Add("UnrecognizedKeyMetadataMissing");
        }

        return violations;
    }
}
