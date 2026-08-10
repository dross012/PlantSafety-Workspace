// ============================================================================
//  Project:        Shared.Authorization.Abstractions
//  Module:         Tenant
//  File:           TenantIsolationCheck.cs
//  Created:        2026-08-10
//  Description:    Phase 11 deliverable. Deterministic, pure tenant-isolation
//                  evaluation, in fixed priority order:
//                    1. No specific facility was requested -> allow (an
//                       unscoped/module-level operation; nothing to check a
//                       boundary against).
//                    2. TenantContext.Unscoped (TenantId == UnscopedTenantId)
//                       -> always allow, regardless of requestedFacilityId or
//                       resolver. This is the honest, documented placeholder
//                       used when no tenant key is configured -- reproducing
//                       this program's pre-Phase-11 behavior exactly, even
//                       for a caller (e.g. RoutingTenant) whose upstream data
//                       already carries a real, non-null FacilityId to check
//                       against. Distinct from a named tenant with no
//                       facility binding (case 4 below), which is NOT
//                       automatically allowed.
//                    3. The TenantContext is bound to one specific facility
//                       -> allow only if it matches the requested facility.
//                    4. The TenantContext is tenant-scoped but not bound to
//                       one facility -> consult the injected
//                       ITenantFacilityAccessResolver, if one was supplied.
//                    5. Otherwise -> deny.
//                  Every downstream module's *Tenant.cs (WorkflowEngineTenant,
//                  RoutingTenant, SchedulingTenant, ...) calls this exact
//                  method; no module reimplements tenant isolation itself.
// ============================================================================
namespace Shared.Authorization.Abstractions.Tenant;

/// <summary>Deterministic, pure tenant-isolation evaluation.</summary>
public static class TenantIsolationCheck
{
    /// <summary>
    /// Evaluates whether <paramref name="tenantContext"/> may access <paramref name="requestedFacilityId"/>.
    /// A null <paramref name="requestedFacilityId"/> always allows (no facility boundary to check).
    /// TenantContext.Unscoped always allows, regardless of the requested facility. A TenantContext
    /// bound to one specific facility only allows a matching request. A tenant-scoped context not
    /// bound to one facility consults <paramref name="tenantFacilityAccessResolver"/>, if supplied.
    /// </summary>
    public static TenantAccessDecision Evaluate(
        TenantContext tenantContext,
        Guid? requestedFacilityId,
        ITenantFacilityAccessResolver? tenantFacilityAccessResolver)
    {
        ArgumentNullException.ThrowIfNull(tenantContext);

        if (requestedFacilityId is null)
        {
            return new TenantAccessDecision(true, "No facility-scoped request; unscoped/module-level access granted.");
        }

        if (tenantContext.TenantId == TenantContext.UnscopedTenantId)
        {
            return new TenantAccessDecision(true, "TenantContext is unscoped (no tenant key configured); access granted regardless of the requested facility.");
        }

        if (tenantContext.FacilityId is Guid boundFacilityId)
        {
            return boundFacilityId == requestedFacilityId.Value
                ? new TenantAccessDecision(true, $"Tenant \"{tenantContext.TenantId}\" is bound to the requested facility {requestedFacilityId.Value}.")
                : new TenantAccessDecision(false, $"Tenant \"{tenantContext.TenantId}\" is bound to facility {boundFacilityId} and cannot access facility {requestedFacilityId.Value}.");
        }

        if (tenantFacilityAccessResolver is not null && tenantFacilityAccessResolver.CanAccessFacility(tenantContext.TenantId, requestedFacilityId.Value))
        {
            return new TenantAccessDecision(true, $"Tenant \"{tenantContext.TenantId}\" is granted access to facility {requestedFacilityId.Value} by the registered access resolver.");
        }

        return new TenantAccessDecision(false, $"Tenant \"{tenantContext.TenantId}\" is not granted access to facility {requestedFacilityId.Value}.");
    }
}
