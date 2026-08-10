// ============================================================================
//  Project:        Shared.Authorization.Abstractions
//  Module:         Identity
//  File:           CallerContextFactory.cs
//  Created:        2026-08-10
//  Description:    Phase 10 deliverable. Deterministic, pure resolution of a
//                  raw "X-Api-Key" header value against a registered
//                  key -> EnterpriseRole map. Never throws for a missing or
//                  unrecognized key -- both fall back to CallerContext.
//                  SystemDefault, exactly the "no endpoint breaks, no
//                  401/403 returned from this middleware" requirement
//                  ApiKeyAuthenticationMiddleware.cs's own header comment
//                  documents; real enforcement happens downstream, in each
//                  module's *Authorization.cs via AuthorizationCheck.
// ============================================================================
using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Roles;

namespace Shared.Authorization.Abstractions.Identity;

/// <summary>Deterministically resolves a raw API-key header value into a CallerContext.</summary>
public static class CallerContextFactory
{
    /// <summary>
    /// Resolves <paramref name="apiKeyHeaderValue"/> against <paramref name="registeredApiKeys"/>.
    /// A null/empty header or an unrecognized key both fall back to CallerContext.SystemDefault --
    /// this method never throws.
    /// </summary>
    public static CallerContextResolution Create(
        string? apiKeyHeaderValue,
        IReadOnlyDictionary<string, EnterpriseRole> registeredApiKeys,
        string correlationId)
    {
        ArgumentNullException.ThrowIfNull(registeredApiKeys);

        if (string.IsNullOrWhiteSpace(apiKeyHeaderValue))
        {
            return new CallerContextResolution(CallerContext.SystemDefault(correlationId), CallerContextResolutionOutcome.NoKeyProvided);
        }

        if (registeredApiKeys.TryGetValue(apiKeyHeaderValue, out EnterpriseRole role))
        {
            return new CallerContextResolution(new CallerContext(role, MfaStatus.NotApplicable, correlationId), CallerContextResolutionOutcome.Recognized);
        }

        return new CallerContextResolution(CallerContext.SystemDefault(correlationId), CallerContextResolutionOutcome.Unrecognized);
    }
}
