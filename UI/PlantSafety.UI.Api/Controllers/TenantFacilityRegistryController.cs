// ============================================================================
//  Project:        PlantSafety.UI.Api
//  Module:         Controllers
//  File:           TenantFacilityRegistryController.cs
//  Created:        2026-08-09
//  Description:    Thin read/write surface over Shared.SafetyData's real
//                  ITenantFacilityRegistryV2 (Phase C.8) -- the
//                  multi-facility-per-tenant registry. Scoped deliberately
//                  to just C.8; Phase C.10's SeverityAware Risk/Compliance
//                  score builders were intentionally NOT wired up here (they
//                  require constructing a full base RiskScore/ComplianceScore,
//                  which spans Shared.SafetyData's much larger Phase 3/5
//                  scoring surface) -- flagged as a known gap in the final
//                  report rather than a silent omission.
// ============================================================================
using Microsoft.AspNetCore.Mvc;

using Shared.SafetyData.Abstractions.Models.Industries.Orchestration;

namespace PlantSafety.UI.Api.Controllers;

/// <summary>Request body for registering a facility to a tenant.</summary>
public sealed record RegisterFacilityRequest(string TenantId, Guid FacilityId, bool IsPrimary);

/// <summary>Exposes Shared.SafetyData's real multi-facility tenant registry (Phase C.8) to PlantSafety.UI.</summary>
[ApiController]
[Route("api/v1/ui/tenant-facility-registry")]
public sealed class TenantFacilityRegistryController(ITenantFacilityRegistryV2 registry) : ControllerBase
{
    /// <summary>GET api/v1/ui/tenant-facility-registry -- every tenant/facility binding.</summary>
    [HttpGet]
    public ActionResult<IReadOnlyList<TenantFacilityMap>> GetAll() => Ok(registry.GetAll());

    /// <summary>GET api/v1/ui/tenant-facility-registry/{tenantId}/dashboard-order -- facility ids ordered primary-first.</summary>
    [HttpGet("{tenantId}/dashboard-order")]
    public ActionResult<IReadOnlyList<Guid>> GetDashboardOrder(string tenantId) => Ok(registry.GetDashboardFacilityIds(tenantId));

    /// <summary>POST api/v1/ui/tenant-facility-registry -- binds a facility to a tenant.</summary>
    [HttpPost]
    public IActionResult Register([FromBody] RegisterFacilityRequest request)
    {
        registry.RegisterFacility(request.TenantId, request.FacilityId, request.IsPrimary);
        return NoContent();
    }

    /// <summary>DELETE api/v1/ui/tenant-facility-registry/{tenantId}/{facilityId} -- unbinds a facility from a tenant.</summary>
    [HttpDelete("{tenantId}/{facilityId:guid}")]
    public IActionResult Unregister(string tenantId, Guid facilityId) =>
        registry.UnregisterFacility(tenantId, facilityId) ? NoContent() : NotFound();
}
