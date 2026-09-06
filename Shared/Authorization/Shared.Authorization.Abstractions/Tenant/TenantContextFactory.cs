// ============================================================================
//  Project:        Shared.Authorization
//  Module:         Abstractions.Tenant
//  File:           TenantContextFactory.cs
//  Created:        2026-08-10
//  Description:    Phase 11 deliverable. Pure, deterministic composition of
//                  the incoming API key + TenantAuthenticationOptions'
//                  registration map + the already-resolved CallerContext
//                  (Phase 9/10) into a TenantContext. No I/O, no exceptions
//                  for the "no key" or "unrecognized key" cases -- both fall
//                  back to TenantContext.Unscoped, exactly preserving
//                  pre-Phase-11 behavior (see TenantContext.cs's own header
//                  comment).
// ============================================================================
namespace Shared.Authorization.Abstractions.Tenant;

/// <summary>Deterministically composes an incoming API key header value and a resolved CallerContext into a TenantContext.</summary>
public static class TenantContextFactory
{
    /// <summary>
    /// Resolves <paramref name="apiKeyHeaderValue"/> against <paramref name="registeredKeyTenants"/>
    /// and returns the resulting TenantContext (falling back to TenantContext.Unscoped when the key
    /// is missing or unrecognized), together with whether the key was provided/recognized.
    /// </summary>
    public static TenantContextResolution Create(
        string? apiKeyHeaderValue,
        IReadOnlyDictionary<string, string> registeredKeyTenants,
        Caller.CallerContext caller,
        string correlationId)
    {
        ArgumentNullException.ThrowIfNull(registeredKeyTenants);
        ArgumentNullException.ThrowIfNull(caller);
        ArgumentNullException.ThrowIfNull(correlationId);

        if (string.IsNullOrWhiteSpace(apiKeyHeaderValue))
        {
            return new TenantContextResolution(TenantContext.Unscoped(caller, correlationId), TenantKeyProvided: false, TenantRecognized: false);
        }

        if (!registeredKeyTenants.TryGetValue(apiKeyHeaderValue, out string? tenantId) || string.IsNullOrWhiteSpace(tenantId))
        {
            return new TenantContextResolution(TenantContext.Unscoped(caller, correlationId), TenantKeyProvided: true, TenantRecognized: false);
        }

        var context = new TenantContext(tenantId, FacilityId: null, caller, correlationId);
        return new TenantContextResolution(context, TenantKeyProvided: true, TenantRecognized: true);
    }
}
