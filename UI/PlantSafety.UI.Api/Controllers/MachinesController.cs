// ============================================================================
//  Project:        PlantSafety.UI.Api
//  Module:         Controllers
//  File:           MachinesController.cs
//  Created:        2026-08-09
//  Description:    Thin read/write surface over Shared.MachineDomain's real
//                  IMachineService (Path C.2) for PlantSafety.UI's Machine
//                  module. No new domain logic -- every method is a direct
//                  pass-through.
// ============================================================================
using Microsoft.AspNetCore.Mvc;

using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.ValueObjects;
using Shared.MachineDomain.Abstractions.Aggregates;
using Shared.MachineDomain.Abstractions.Enums;
using Shared.MachineDomain.Abstractions.Location;
using Shared.MachineDomain.Abstractions.Repositories;
using Shared.MachineDomain.Abstractions.ValueObjects;

namespace PlantSafety.UI.Api.Controllers;

/// <summary>Request body for creating or updating a machine's core metadata.</summary>
public sealed record MachineMetadataRequest(string Name, MachineType MachineType, Guid FacilityId, string AreaDescription);

/// <summary>Request body for updating a machine's maintenance profile.</summary>
public sealed record MachineMaintenanceRequest(DateTime? LastMaintenanceAtUtc, DateTime? NextScheduledMaintenanceAtUtc, int MaintenanceIntervalDays);

/// <summary>Request body for recording a machine failure.</summary>
public sealed record MachineFailureRequest(string Description, HazardType ResultingHazardType);

/// <summary>Exposes Shared.MachineDomain's real Machine aggregate to PlantSafety.UI.</summary>
[ApiController]
[Route("api/v1/ui/machines")]
public sealed class MachinesController(IMachineService machineService) : ControllerBase
{
    /// <summary>GET api/v1/ui/machines -- every machine.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MachineState>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await machineService.GetAllMachinesAsync(cancellationToken));

    /// <summary>GET api/v1/ui/machines/{id} -- a single machine.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MachineState>> GetById(Guid id, CancellationToken cancellationToken)
    {
        MachineState? state = await machineService.GetMachineAsync(MachineId.From(id), cancellationToken);
        return state is null ? NotFound() : Ok(state);
    }

    /// <summary>POST api/v1/ui/machines -- creates a new machine.</summary>
    [HttpPost]
    public async Task<ActionResult<MachineState>> Create([FromBody] MachineMetadataRequest request, CancellationToken cancellationToken)
    {
        var location = new MachineLocation(FacilityId.From(request.FacilityId), request.AreaDescription);
        MachineState created = await machineService.CreateMachineAsync(request.Name, request.MachineType, location, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.MachineId.Value }, created);
    }

    /// <summary>PUT api/v1/ui/machines/{id}/metadata -- updates a machine's name/type/location.</summary>
    [HttpPut("{id:guid}/metadata")]
    public async Task<ActionResult<MachineState>> UpdateMetadata(Guid id, [FromBody] MachineMetadataRequest request, CancellationToken cancellationToken)
    {
        var location = new MachineLocation(FacilityId.From(request.FacilityId), request.AreaDescription);
        MachineState? updated = await machineService.UpdateMachineMetadataAsync(MachineId.From(id), request.Name, request.MachineType, location, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    /// <summary>PUT api/v1/ui/machines/{id}/maintenance -- replaces a machine's maintenance profile.</summary>
    [HttpPut("{id:guid}/maintenance")]
    public async Task<ActionResult<MachineState>> UpdateMaintenance(Guid id, [FromBody] MachineMaintenanceRequest request, CancellationToken cancellationToken)
    {
        MachineState? updated = await machineService.UpdateMaintenanceProfileAsync(
            MachineId.From(id), request.LastMaintenanceAtUtc, request.NextScheduledMaintenanceAtUtc, request.MaintenanceIntervalDays, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    /// <summary>POST api/v1/ui/machines/{id}/failures -- records a machine failure.</summary>
    [HttpPost("{id:guid}/failures")]
    public async Task<ActionResult<MachineState>> RecordFailure(Guid id, [FromBody] MachineFailureRequest request, CancellationToken cancellationToken)
    {
        MachineState? updated = await machineService.RecordFailureAsync(MachineId.From(id), request.Description, request.ResultingHazardType, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }
}
