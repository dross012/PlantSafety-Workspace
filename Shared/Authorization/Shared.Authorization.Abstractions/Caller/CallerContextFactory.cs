// ============================================================================
//  Project:        Shared.Authorization
//  Module:         Abstractions.Caller
//  File:           CallerContextFactory.cs
//  Created:        2026-08-10
//  Description:    Phase 10 deliverable. Pure, deterministic composition of
//                  ApiKeyValidator + CallerContext.SystemDefault into the
//                  request's resolved CallerContext. No I/O, no exceptions
//                  for the "no key" or "unrecognized key" cases -- both fall
//                  back to CallerContext.SystemDefault (EnterpriseRole.
//                  PlatformAdmin), exactly preserving today's observable
//                  behavior (see CallerContext.cs's own header comment and
//                  this phase's explicit "no endpoint breaks / no 401
//                  returned" requirement). MfaStatus is always
//                  MfaStatus.NotApplicable -- this phase does not implement
//                  MFA flows at the API layer, matching the prompt's own
//                  "Does not require MFA flows at the API layer" constraint;
//                  a recognized API key is not evidence of a second factor.
// ============================================================================
using Shared.Authorization.Abstractions.Identity;
using Shared.Authorization.Abstractions.Roles;

namespace Shared.Authorization.Abstractions.Caller;

/// <summary>Deterministically composes an incoming API key header value into a resolved CallerContext.</summary>
public static class CallerContextFactory
{
    /// <summary>
    /// Resolves <paramref name="apiKeyHeaderValue"/> against <paramref name="registeredKeys"/> and
    /// returns the resulting CallerContext (falling back to CallerContext.SystemDefault when the
    /// key is missing or unrecognized) together with the ApiKeyIdentity it was derived from.
    /// </summary>
    public static CallerContextResolution Create(
        string? apiKeyHeaderValue,
        IReadOnlyDictionary<string, EnterpriseRole> registeredKeys,
        string correlationId)
    {
        ArgumentNullException.ThrowIfNull(registeredKeys);
        ArgumentNullException.ThrowIfNull(correlationId);

        EnterpriseRole systemDefaultRole = CallerContext.SystemDefault(correlationId).Role;

        if (string.IsNullOrWhiteSpace(apiKeyHeaderValue))
        {
            ApiKeyIdentity identity = ApiKeyIdentity.NoKeyProvided(systemDefaultRole);
            return new CallerContextResolution(CallerContext.SystemDefault(correlationId), identity);
        }

        EnterpriseRole? resolvedRole = ApiKeyValidator.Resolve(apiKeyHeaderValue, registeredKeys);
        if (resolvedRole is null)
        {
            ApiKeyIdentity identity = ApiKeyIdentity.UnrecognizedKey(systemDefaultRole);
            return new CallerContextResolution(CallerContext.SystemDefault(correlationId), identity);
        }

        var caller = new CallerContext(resolvedRole, MfaStatus.NotApplicable, correlationId);
        return new CallerContextResolution(caller, ApiKeyIdentity.Recognized(resolvedRole));
    }
}
