// ============================================================================
//  Project:        PlantSafety.UI.Api
//  Module:         Controllers
//  File:           FacilitiesController.cs
//  Created:        2026-08-09
//  Description:    Thin read/write surface over Shared.FacilityDomain's real
//                  IFacilityService (Path B.1/B.2, extended through Path
//                  C.5/C.12) for PlantSafety.UI's Facility module. No new
//                  domain logic -- every method is a direct pass-through to
//                  IFacilityService, matching the "UI services are the only
//                  layer allowed to call backend APIs, backend APIs are thin
//                  wrappers over Application services" rule.
// ============================================================================
using Microsoft.AspNetCore.Mvc;

using Shared.FacilityDomain.Abstractions.Aggregates;
using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.Hierarchy;
using Shared.FacilityDomain.Abstractions.Repositories;
using Shared.FacilityDomain.Abstractions.ValueObjects;

namespace PlantSafety.UI.Api.Controllers;

/// <summary>Request body for creating or updating a facility's core metadata.</summary>
public sealed record FacilityMetadataRequest(string Name, string IndustryName, string? NaicsCode, FacilityLocation Location);

/// <summary>Request body for a shutdown/decommission action requiring a reason.</summary>
public sealed record FacilityReasonRequest(string Reason);

/// <summary>Request body for adding a building/area/zone to a facility's hierarchy.</summary>
public sealed record FacilityHierarchyNodeRequest(Guid? ParentId, string Name);

/// <summary>Exposes Shared.FacilityDomain's real Facility aggregate to PlantSafety.UI.</summary>
[ApiController]
[Route("api/v1/ui/facilities")]
public sealed class FacilitiesController(IFacilityService facilityService) : ControllerBase
{
    /// <summary>GET api/v1/ui/facilities -- every facility.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FacilityState>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await facilityService.GetAllFacilitiesAsync(cancellationToken));

    /// <summary>GET api/v1/ui/facilities/{id} -- a single facility.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FacilityState>> GetById(Guid id, CancellationToken cancellationToken)
    {
        FacilityState? state = await facilityService.GetFacilityAsync(FacilityId.From(id), cancellationToken);
        return state is null ? NotFound() : Ok(state);
    }

    /// <summary>POST api/v1/ui/facilities -- creates a new facility.</summary>
    [HttpPost]
    public async Task<ActionResult<FacilityState>> Create([FromBody] FacilityMetadataRequest request, CancellationToken cancellationToken)
    {
        FacilityState created = await facilityService.CreateFacilityAsync(
            request.Name, new FacilityIndustry(request.IndustryName, request.NaicsCode), request.Location, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.FacilityId.Value }, created);
    }

    /// <summary>PUT api/v1/ui/facilities/{id}/metadata -- updates a facility's name/industry/location.</summary>
    [HttpPut("{id:guid}/metadata")]
    public async Task<ActionResult<FacilityState>> UpdateMetadata(Guid id, [FromBody] FacilityMetadataRequest request, CancellationToken cancellationToken)
    {
        FacilityState? updated = await facilityService.UpdateFacilityMetadataAsync(
            FacilityId.From(id), request.Name, new FacilityIndustry(request.IndustryName, request.NaicsCode), request.Location, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    /// <summary>PUT api/v1/ui/facilities/{id}/hazard-profile -- replaces a facility's active hazard types.</summary>
    [HttpPut("{id:guid}/hazard-profile")]
    public async Task<ActionResult<FacilityState>> UpdateHazardProfile(Guid id, [FromBody] IReadOnlyList<HazardType> hazardTypes, CancellationToken cancellationToken)
    {
        FacilityState? updated = await facilityService.UpdateHazardProfileAsync(FacilityId.From(id), hazardTypes, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    /// <summary>POST api/v1/ui/facilities/{id}/buildings -- adds a building to the facility's hierarchy.</summary>
    [HttpPost("{id:guid}/buildings")]
    public async Task<ActionResult<FacilityState>> AddBuilding(Guid id, [FromBody] FacilityHierarchyNodeRequest request, CancellationToken cancellationToken)
    {
        FacilityState? updated = await facilityService.AddBuildingAsync(FacilityId.From(id), request.Name, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    /// <summary>POST api/v1/ui/facilities/{id}/areas -- adds an area under an existing building.</summary>
    [HttpPost("{id:guid}/areas")]
    public async Task<ActionResult<FacilityState>> AddArea(Guid id, [FromBody] FacilityHierarchyNodeRequest request, CancellationToken cancellationToken)
    {
        if (request.ParentId is null)
        {
            return BadRequest("ParentId (BuildingId) is required.");
        }

        FacilityState? updated = await facilityService.AddAreaAsync(FacilityId.From(id), BuildingId.From(request.ParentId.Value), request.Name, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    /// <summary>POST api/v1/ui/facilities/{id}/zones -- adds a zone under an existing area.</summary>
    [HttpPost("{id:guid}/zones")]
    public async Task<ActionResult<FacilityState>> AddZone(Guid id, [FromBody] FacilityHierarchyNodeRequest request, CancellationToken cancellationToken)
    {
        if (request.ParentId is null)
        {
            return BadRequest("ParentId (AreaId) is required.");
        }

        FacilityState? updated = await facilityService.AddZoneAsync(FacilityId.From(id), AreaId.From(request.ParentId.Value), request.Name, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    /// <summary>POST api/v1/ui/facilities/{id}/activate.</summary>
    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult<FacilityState>> Activate(Guid id, CancellationToken cancellationToken)
    {
        FacilityState? updated = await facilityService.ActivateFacilityAsync(FacilityId.From(id), cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    /// <summary>POST api/v1/ui/facilities/{id}/shutdown.</summary>
    [HttpPost("{id:guid}/shutdown")]
    public async Task<ActionResult<FacilityState>> Shutdown(Guid id, [FromBody] FacilityReasonRequest request, CancellationToken cancellationToken)
    {
        FacilityState? updated = await facilityService.ShutdownFacilityAsync(FacilityId.From(id), request.Reason, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    /// <summary>POST api/v1/ui/facilities/{id}/reopen.</summary>
    [HttpPost("{id:guid}/reopen")]
    public async Task<ActionResult<FacilityState>> Reopen(Guid id, CancellationToken cancellationToken)
    {
        FacilityState? updated = await facilityService.ReopenFacilityAsync(FacilityId.From(id), cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    /// <summary>POST api/v1/ui/facilities/{id}/decommission.</summary>
    [HttpPost("{id:guid}/decommission")]
    public async Task<ActionResult<FacilityState>> Decommission(Guid id, [FromBody] FacilityReasonRequest request, CancellationToken cancellationToken)
    {
        FacilityState? updated = await facilityService.DecommissionFacilityAsync(FacilityId.From(id), request.Reason, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }
}
