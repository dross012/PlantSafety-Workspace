// ============================================================================
//  Project:        Shared.Authorization.Abstractions
//  Module:         Tenant
//  File:           TenantContextFactory.cs
//  Created:        2026-08-10
//  Description:    Phase 11 deliverable. Deterministic, pure resolution of a
//                  raw "X-Api-Key" header value against a registered
//                  key -> TenantId map -- independent of Phase 10's
//                  key -> EnterpriseRole map (a key may be registered for
//                  one, both, or neither). Never throws for a missing or
//                  unrecognized key -- both fall back to TenantContext.
//                  Unscoped, mirroring CallerContextFactory exactly.
// ============================================================================
using Shared.Authorization.Abstractions.Caller;

namespace Shared.Authorization.Abstractions.Tenant;

/// <summary>Deterministically resolves a raw API-key header value into a TenantContext.</summary>
public static class TenantContextFactory
{
    /// <summary>
    /// Resolves <paramref name="apiKeyHeaderValue"/> against <paramref name="registeredKeyTenants"/>.
    /// A null/empty header or an unrecognized key both fall back to TenantContext.Unscoped -- this
    /// method never throws. The resolved TenantContext carries no FacilityId; a specific facility
    /// binding is only ever established by the caller passing requestedFacilityId to
    /// TenantIsolationCheck.Evaluate.
    /// </summary>
    public static TenantContextResolution Create(
        string? apiKeyHeaderValue,
        IReadOnlyDictionary<string, string> registeredKeyTenants,
        CallerContext caller,
        string correlationId)
    {
        ArgumentNullException.ThrowIfNull(registeredKeyTenants);
        ArgumentNullException.ThrowIfNull(caller);

        if (string.IsNullOrWhiteSpace(apiKeyHeaderValue))
        {
            return new TenantContextResolution(TenantContext.Unscoped(caller, correlationId), TenantContextResolutionOutcome.NoKeyProvided);
        }

        if (registeredKeyTenants.TryGetValue(apiKeyHeaderValue, out string? tenantId))
        {
            return new TenantContextResolution(new TenantContext(tenantId, FacilityId: null, caller, correlationId), TenantContextResolutionOutcome.Recognized);
        }

        return new TenantContextResolution(TenantContext.Unscoped(caller, correlationId), TenantContextResolutionOutcome.Unrecognized);
    }
}
