// ============================================================================
//  Project:        Shared.Authorization.Infrastructure
//  Module:         Governance
//  File:           AuthorizationGovernanceRules.cs
//  Created:        2026-08-10
//  Description:    Phase 9 deliverable. Pure validation over authorization
//                  metadata -- CallerContext shape, permission-string shape,
//                  role-inheritance consistency. Zero side effects, mirrors
//                  Shared.Telemetry.Abstractions.Governance.
//                  TelemetryGovernanceRules' shape exactly (Phase 8):
//                  input in, violation list out. Does not govern anything
//                  outside authorization -- no orchestration/scheduling/
//                  routing/diagnostics rules, since Phase 7/8 already
//                  established none of those has any real execution path in
//                  this program.
// ============================================================================
using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Roles;

namespace Shared.Authorization.Infrastructure.Governance;

/// <summary>Pure validation rules for authorization metadata: CallerContext shape and permission-string shape.</summary>
public static class AuthorizationGovernanceRules
{
    /// <summary>Validates a CallerContext's shape. Returns every violation found (empty if valid).</summary>
    public static IReadOnlyList<string> ValidateCallerContext(CallerContext? caller)
    {
        var violations = new List<string>();

        if (caller is null)
        {
            violations.Add("CallerContext is missing -- every authorization-checked operation requires one.");
            return violations;
        }

        if (string.IsNullOrWhiteSpace(caller.CorrelationId))
        {
            violations.Add("CallerContext.CorrelationId is missing or empty -- correlation propagation is broken for this authorization check.");
        }

        if (!EnterpriseRole.All.Contains(caller.Role))
        {
            violations.Add($"CallerContext.Role \"{caller.Role}\" is not one of the canonical EnterpriseRole.All values -- no dynamic/invented roles are permitted.");
        }

        return violations;
    }

    /// <summary>Validates a module-qualified permission string's shape ("{Module}.{Verb}", both non-empty).</summary>
    public static IReadOnlyList<string> ValidatePermissionShape(string module, string requiredVerb)
    {
        var violations = new List<string>();

        if (string.IsNullOrWhiteSpace(module))
        {
            violations.Add("Module name is missing or empty.");
        }

        if (string.IsNullOrWhiteSpace(requiredVerb))
        {
            violations.Add("Required permission verb is missing or empty.");
        }

        return violations;
    }

    /// <summary>
    /// Validates that a role-inheritance relationship holds deterministically (higher-or-equal
    /// rank always satisfies a lower-or-equal-rank requirement, never the reverse) -- guards
    /// against a future change accidentally introducing "first match wins" or priority-override
    /// semantics into RoleInheritanceResolver.Satisfies.
    /// </summary>
    public static IReadOnlyList<string> ValidateRoleInheritanceConsistency(EnterpriseRole held, EnterpriseRole required)
    {
        var violations = new List<string>();

        bool satisfies = RoleInheritanceResolver.Satisfies(held, required);
        bool expectedBasedOnRank = held.Rank >= required.Rank;

        if (satisfies != expectedBasedOnRank)
        {
            violations.Add($"RoleInheritanceResolver.Satisfies({held.Name}, {required.Name}) returned {satisfies}, but rank comparison expects {expectedBasedOnRank} -- role inheritance is no longer purely rank-deterministic.");
        }

        return violations;
    }
}
