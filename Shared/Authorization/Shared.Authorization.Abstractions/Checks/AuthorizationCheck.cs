// ============================================================================
//  Project:        Shared.Authorization.Abstractions
//  Module:         Checks
//  File:           AuthorizationCheck.cs
//  Created:        2026-08-10
//  Description:    Phase 9 deliverable. Deterministic, pure permission
//                  check: builds the "{module}.{verb}" permission string,
//                  checks it against the caller's resolved permission set
//                  (via the injected IPermissionResolver), and additionally
//                  requires MfaStatus.Verified for ModulePermissionVerb.
//                  Admin -- the one MFA-gated tier (PlantSafetyMetrics.
//                  MfaRequirementFailures, Phase 9). Every downstream
//                  module's *Authorization.cs (WorkflowEngineAuthorization,
//                  RoutingAuthorization, SchedulingAuthorization, ...) calls
//                  this exact method for "{Module}.Read"; no module
//                  reimplements the check itself.
// ============================================================================
using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Permissions;
using Shared.Authorization.Abstractions.Roles;

namespace Shared.Authorization.Abstractions.Checks;

/// <summary>Deterministic, pure permission check against a caller's resolved permission set.</summary>
public static class AuthorizationCheck
{
    /// <summary>
    /// Evaluates whether <paramref name="caller"/> is authorized for "{<paramref name="module"/>}.
    /// {<paramref name="verb"/>}". ModulePermissionVerb.Admin additionally requires MfaStatus.Verified
    /// -- caught before consulting <paramref name="permissionResolver"/>, and flagged via
    /// AuthorizationDecision.MfaRequirementFailed.
    /// </summary>
    public static AuthorizationDecision Evaluate(
        CallerContext caller,
        IPermissionResolver permissionResolver,
        string module,
        ModulePermissionVerb verb)
    {
        ArgumentNullException.ThrowIfNull(caller);
        ArgumentNullException.ThrowIfNull(permissionResolver);
        ArgumentNullException.ThrowIfNull(module);

        string permission = $"{module}.{verb}";

        if (verb == ModulePermissionVerb.Admin && caller.MfaStatus != MfaStatus.Verified)
        {
            return new AuthorizationDecision(
                IsAuthorized: false,
                Reason: $"\"{permission}\" requires MFA verification, but caller's MfaStatus is \"{caller.MfaStatus}\".",
                MfaRequirementFailed: true);
        }

        IReadOnlySet<string> granted = permissionResolver.Resolve(caller.Role, module);
        return granted.Contains(permission)
            ? new AuthorizationDecision(true, $"Caller role \"{caller.Role}\" grants \"{permission}\".")
            : new AuthorizationDecision(false, $"Caller role \"{caller.Role}\" does not grant \"{permission}\".");
    }
}
