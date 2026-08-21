// ============================================================================
//  Project:        Shared.Authorization
//  Module:         Abstractions.Tenant
//  File:           ITenantFacilityAccessResolver.cs
//  Created:        2026-08-10
//  Description:    Phase 11 deliverable. The seam TenantIsolationCheck uses
//                  to ask "is this facility bound to this tenant?" without
//                  Shared.Authorization taking a direct dependency on
//                  Shared.SafetyData (a separate bounded context with its own
//                  real tenant/facility data -- ITenantFacilityRegistryV2,
//                  Phase C.8). The composition root (PlantSafety.UI.Api)
//                  supplies the real implementation, backed by
//                  ITenantFacilityRegistryV2.CanRouteToFacility -- this
//                  interface is not a second tenant/facility data store, just
//                  the query contract Authorization needs.
// ============================================================================
namespace Shared.Authorization.Abstractions.Tenant;

/// <summary>Resolves whether a facility is bound to a tenant, without Shared.Authorization depending on the data source.</summary>
public interface ITenantFacilityAccessResolver
{
    /// <summary>True if <paramref name="facilityId"/> is bound to <paramref name="tenantId"/>.</summary>
    bool CanAccessFacility(string tenantId, Guid facilityId);
}
