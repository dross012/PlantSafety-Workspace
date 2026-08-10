// ============================================================================
//  Project:        Shared.Authorization.Abstractions
//  Module:         Tenant
//  File:           TenantContext.cs
//  Created:        2026-08-10
//  Description:    Phase 11 deliverable. The resolved tenant/facility scope
//                  every tenant-isolation check takes as input.
//                  TenantContext.Unscoped is the honest, documented
//                  placeholder used when no tenant key is configured or the
//                  presented key is unrecognized -- TenantIsolationCheck
//                  always allows an unscoped context, reproducing this
//                  program's pre-Phase-11 behavior exactly.
// ============================================================================
using Shared.Authorization.Abstractions.Caller;

namespace Shared.Authorization.Abstractions.Tenant;

/// <summary>The resolved tenant/facility scope every tenant-isolation check takes as input.</summary>
public sealed record TenantContext(string TenantId, Guid? FacilityId, CallerContext Caller, string CorrelationId)
{
    /// <summary>The TenantId used by TenantContext.Unscoped -- never a real tenant identifier.</summary>
    public const string UnscopedTenantId = "unscoped";

    /// <summary>
    /// The honest, documented placeholder tenant context used when no tenant key is configured, no
    /// key is presented, or the presented key is unrecognized. TenantIsolationCheck always allows
    /// this context, reproducing this program's pre-Phase-11 behavior exactly.
    /// </summary>
    public static TenantContext Unscoped(CallerContext caller, string correlationId) => new(UnscopedTenantId, FacilityId: null, caller, correlationId);
}
