// ============================================================================
//  Project:        Shared.Authorization
//  Module:         Abstractions.Checks
//  File:           AuthorizationCheck.cs
//  Created:        2026-08-10
//  Description:    Phase 9 deliverable. The single, deterministic
//                  permission-check function every domain-level
//                  authorization wrapper (WorkflowEngineAuthorization,
//                  FacilityAuthorization, AuditAuthorization) calls. Pure
//                  function of (CallerContext, module, required verb) ->
//                  AuthorizationDecision -- no I/O, no randomness, uses
//                  only the existing RoleInheritanceResolver/
//                  IPermissionResolver primitives. Deliberately does NOT
//                  implement "first match wins," priority overrides,
//                  weighted scoring, or any prohibited pattern from this
//                  phase's rules -- a caller is authorized if and only if
//                  their role's resolved permission set (Shared.
//                  Authorization's existing, real EnterprisePermissionResolver)
//                  contains the exact "{module}.{verb}" permission string.
// ============================================================================
using Shared.Authorization.Abstractions.Permissions;
using Shared.Authorization.Abstractions.Roles;

namespace Shared.Authorization.Abstractions.Checks;

/// <summary>Deterministically checks whether a CallerContext is authorized for a module-scoped permission verb.</summary>
public static class AuthorizationCheck
{
    /// <summary>
    /// Resolves <paramref name="caller"/>'s permission set for <paramref name="module"/> via
    /// <paramref name="permissionResolver"/> and returns Allow if it contains
    /// "{module}.{requiredVerb}", Deny otherwise. Never throws for an unauthorized caller --
    /// authorization failure is a value, not an exception.
    /// </summary>
    public static AuthorizationDecision Evaluate(
        Caller.CallerContext caller,
        IPermissionResolver permissionResolver,
        string module,
        ModulePermissionVerb requiredVerb)
    {
        ArgumentNullException.ThrowIfNull(caller);
        ArgumentNullException.ThrowIfNull(permissionResolver);
        ArgumentNullException.ThrowIfNull(module);

        IReadOnlySet<string> granted = permissionResolver.Resolve(caller.Role, module);
        string requiredPermission = $"{module}.{requiredVerb}";

        return granted.Contains(requiredPermission)
            ? AuthorizationDecision.Allow($"Role \"{caller.Role.Name}\" grants \"{requiredPermission}\".")
            : AuthorizationDecision.Deny($"Role \"{caller.Role.Name}\" does not grant \"{requiredPermission}\".");
    }

    /// <summary>
    /// Evaluates <see cref="Evaluate"/> and additionally requires <paramref name="caller"/>'s
    /// MfaStatus to be Verified. Used for operations this program's MfaPolicy.RequireMfa
    /// policy name is meant to gate -- see MfaPolicy.cs's own header comment for why the
    /// actual claim-checking logic lives outside this repository today.
    /// </summary>
    public static AuthorizationDecision EvaluateWithMfa(
        Caller.CallerContext caller,
        IPermissionResolver permissionResolver,
        string module,
        ModulePermissionVerb requiredVerb)
    {
        AuthorizationDecision baseDecision = Evaluate(caller, permissionResolver, module, requiredVerb);
        if (!baseDecision.IsAuthorized)
        {
            return baseDecision;
        }

        return caller.MfaStatus == Caller.MfaStatus.Verified
            ? AuthorizationDecision.Allow(baseDecision.Reason + " MFA verified.")
            : AuthorizationDecision.Deny($"MFA required but caller's MfaStatus is \"{caller.MfaStatus}\", not Verified.");
    }
}
