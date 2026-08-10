// ============================================================================
//  Project:        Shared.Authorization.Abstractions
//  Module:         Tenant
//  File:           ITenantFacilityAccessResolver.cs
//  Created:        2026-08-10
//  Description:    Phase 11 deliverable. The seam TenantIsolationCheck
//                  consults when a TenantContext is tenant-scoped but not
//                  bound to one specific FacilityId. Deliberately an
//                  interface with no implementation in this project --
//                  PlantSafety.UI.Api's TenantFacilityAccessResolver adapts
//                  the real Shared.SafetyData.Abstractions.Models.Industries.
//                  Orchestration.ITenantFacilityRegistryV2 to this shape, at
//                  the composition root, so Shared.Authorization never takes
//                  a dependency on Shared.SafetyData.
// ============================================================================
namespace Shared.Authorization.Abstractions.Tenant;

/// <summary>Resolves whether a tenant may access a specific facility. Implemented at the composition root, not in Shared.Authorization.</summary>
public interface ITenantFacilityAccessResolver
{
    /// <summary>Returns true if <paramref name="tenantId"/> may access <paramref name="facilityId"/>.</summary>
    bool CanAccessFacility(string tenantId, Guid facilityId);
}
