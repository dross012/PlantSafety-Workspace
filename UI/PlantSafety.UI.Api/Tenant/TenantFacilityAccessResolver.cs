// ============================================================================
//  Project:        PlantSafety.UI.Api
//  Module:         Tenant
//  File:           TenantFacilityAccessResolver.cs
//  Created:        2026-08-10
//  Description:    Phase 11 deliverable. The composition-root adapter
//                  implementing Shared.Authorization.Abstractions.Tenant.
//                  ITenantFacilityAccessResolver over the real, existing
//                  Shared.SafetyData.Abstractions.Models.Industries.
//                  Orchestration.ITenantFacilityRegistryV2 (Phase C.8,
//                  already registered in this host). Lives here, not in
//                  Shared.Authorization, so Shared.Authorization never takes
//                  a dependency on Shared.SafetyData -- see
//                  ITenantFacilityAccessResolver.cs's own header comment.
// ============================================================================
using Shared.Authorization.Abstractions.Tenant;
using Shared.SafetyData.Abstractions.Models.Industries.Orchestration;

namespace PlantSafety.UI.Api.Tenant;

/// <summary>Adapts ITenantFacilityRegistryV2's real tenant/facility binding data to ITenantFacilityAccessResolver.</summary>
public sealed class TenantFacilityAccessResolver(ITenantFacilityRegistryV2 registry) : ITenantFacilityAccessResolver
{
    /// <inheritdoc />
    public bool CanAccessFacility(string tenantId, Guid facilityId) => registry.CanRouteToFacility(tenantId, facilityId);
}
