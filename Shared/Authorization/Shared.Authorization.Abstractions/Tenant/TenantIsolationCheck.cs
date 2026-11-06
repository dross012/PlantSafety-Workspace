// ============================================================================
//  Project:        Shared.Authorization
//  Module:         Abstractions.Tenant
//  File:           TenantIsolationCheck.cs
//  Created:        2026-08-10
//  Description:    Phase 11 deliverable. Pure decision function enforcing
//                  cross-tenant/cross-facility isolation, mirroring
//                  Shared.Authorization.Abstractions.Checks.AuthorizationCheck
//                  exactly (Phase 9's own pattern): a static Evaluate method,
//                  no I/O, no side effects.
//
//                  Deterministic rules, in order:
//                  1. An Unscoped TenantContext (no tenant binding configured
//                     for this caller) always allows -- there is no boundary
//                     to enforce yet (see TenantContext.Unscoped's own header
//                     comment for why this is not a privileged bypass).
//                  2. A null requestedFacilityId (the operation is not
//                     facility-scoped, e.g. WorkflowContext carries no
//                     FacilityId field today) always allows -- there is
//                     nothing to check the boundary against.
//                  3. If the TenantContext itself already carries a
//                     FacilityId, the requested facility must match it
//                     exactly (a caller scoped to one facility cannot act on
//                     another).
//                  4. Otherwise, ITenantFacilityAccessResolver.CanAccessFacility
//                     is consulted -- the real Shared.SafetyData
//                     ITenantFacilityRegistryV2 binding data, via the seam
//                     defined in ITenantFacilityAccessResolver.cs.
// ============================================================================
namespace Shared.Authorization.Abstractions.Tenant;

/// <summary>Deterministically evaluates whether a TenantContext may access a given facility.</summary>
public static class TenantIsolationCheck
{
    /// <summary>
    /// Evaluates whether <paramref name="tenantContext"/> may access <paramref name="requestedFacilityId"/>.
    /// See this file's own header comment for the exact rule ordering.
    /// </summary>
    public static TenantAccessDecision Evaluate(
        TenantContext tenantContext,
        Guid? requestedFacilityId,
        ITenantFacilityAccessResolver? resolver)
    {
        ArgumentNullException.ThrowIfNull(tenantContext);

        if (tenantContext.TenantId == TenantContext.UnscopedTenantId)
        {
            return TenantAccessDecision.Allow("No tenant binding configured for this caller; no boundary to enforce.");
        }

        if (requestedFacilityId is not { } facilityId)
        {
            return TenantAccessDecision.Allow("Operation is not facility-scoped; no boundary to check.");
        }

        if (tenantContext.FacilityId is { } scopedFacilityId)
        {
            return scopedFacilityId == facilityId
                ? TenantAccessDecision.Allow($"Requested facility matches tenant \"{tenantContext.TenantId}\"'s scoped facility.")
                : TenantAccessDecision.Deny($"Tenant \"{tenantContext.TenantId}\" is scoped to facility {scopedFacilityId}, not {facilityId}.");
        }

        if (resolver is null)
        {
            return TenantAccessDecision.Deny($"Tenant \"{tenantContext.TenantId}\" has no facility access resolver configured to verify facility {facilityId}.");
        }

        return resolver.CanAccessFacility(tenantContext.TenantId, facilityId)
            ? TenantAccessDecision.Allow($"Facility {facilityId} is bound to tenant \"{tenantContext.TenantId}\".")
            : TenantAccessDecision.Deny($"Facility {facilityId} is not bound to tenant \"{tenantContext.TenantId}\".");
    }
}
