// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Application.Enterprise.MultiRegion
//  Layer:          Application
//  File:           ITenantRegionRouter.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Phase 7 §4.3 (Global Routing) abstraction over "which region should this
//                  tenant's request be routed/processed in". Real, deterministic, testable
//                  domain-adjacent logic; the actual network-level steering of a request to the
//                  resolved region (a real Traffic Manager/Front Door/global load balancer) is
//                  infrastructure this codebase cannot provision -- see
//                  docs\Phase7\multi-region-architecture.md.
//
//                  INTEGRATION POINT FOR THE TENANT AGGREGATE: at the time this was built, the
//                  concurrent Tenancy agent's `PlantSafety.Domain.Tenancy.Tenant` aggregate had
//                  not yet landed (verified: no `PlantSafety.Domain\Tenancy\` folder exists as of
//                  this pass). This interface is therefore built against a bare `Guid TenantId`
//                  rather than the Tenant aggregate itself. Once Tenant lands (presumably
//                  carrying its own region-affinity value on the aggregate), the recommended
//                  migration path is: (1) keep this interface's Guid-based signature as the
//                  primary contract (callers throughout the codebase pass TenantId around as a
//                  bare Guid via the ICommand/IQuery envelope convention already), and (2) have
//                  `TenantRegionAffinityResolver` (or a successor) additionally consult
//                  `Tenant.RegionAffinity` (or equivalent) as its first-choice source of truth,
//                  falling back to today's deterministic hash-based assignment only for tenants
//                  that have not yet had an explicit region set on the aggregate. Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.SharedKernel.Contracts.Enterprise.MultiRegion;

namespace PlantSafety.Application.Enterprise.MultiRegion;

/// <summary>
/// Determines which <see cref="Region"/> a given tenant's request should be routed to/processed
/// in. Deterministic: the same tenant always resolves to the same region (region assignment is
/// never randomly re-decided per call).
/// </summary>
public interface ITenantRegionRouter
{
    /// <summary>
    /// Resolves the <see cref="Region"/> that <paramref name="tenantId"/>'s requests should be
    /// routed to/processed in.
    /// </summary>
    /// <param name="tenantId">The tenant to resolve a region for.</param>
    Region ResolveRegion(Guid tenantId);
}
