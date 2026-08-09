// ============================================================================
//  Project:        PlantSafety.UI.Api
//  Module:         Controllers
//  File:           EmployeesController.cs
//  Created:        2026-08-09
//  Description:    Thin read/write surface over Shared.EmployeeDomain's real
//                  IEmployeeService (Path C.1) for PlantSafety.UI's Employee
//                  module. No new domain logic -- every method is a direct
//                  pass-through.
// ============================================================================
using Microsoft.AspNetCore.Mvc;

using Shared.EmployeeDomain.Abstractions.Aggregates;
using Shared.EmployeeDomain.Abstractions.Enums;
using Shared.EmployeeDomain.Abstractions.Exposure;
using Shared.EmployeeDomain.Abstractions.Repositories;
using Shared.EmployeeDomain.Abstractions.Training;
using Shared.EmployeeDomain.Abstractions.ValueObjects;

namespace PlantSafety.UI.Api.Controllers;

/// <summary>Request body for creating or updating an employee's core metadata.</summary>
public sealed record EmployeeMetadataRequest(string Name, EmployeeRole Role);

/// <summary>Request body for replacing an employee's completed training codes.</summary>
public sealed record EmployeeTrainingRequest(IReadOnlyList<string> CompletedTrainingCodes);

/// <summary>Request body for recording an injury.</summary>
public sealed record EmployeeInjuryRequest(string Description);

/// <summary>Exposes Shared.EmployeeDomain's real Employee aggregate to PlantSafety.UI.</summary>
[ApiController]
[Route("api/v1/ui/employees")]
public sealed class EmployeesController(IEmployeeService employeeService) : ControllerBase
{
    /// <summary>GET api/v1/ui/employees -- every employee.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EmployeeState>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await employeeService.GetAllEmployeesAsync(cancellationToken));

    /// <summary>GET api/v1/ui/employees/{id} -- a single employee.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EmployeeState>> GetById(Guid id, CancellationToken cancellationToken)
    {
        EmployeeState? state = await employeeService.GetEmployeeAsync(EmployeeId.From(id), cancellationToken);
        return state is null ? NotFound() : Ok(state);
    }

    /// <summary>POST api/v1/ui/employees -- creates a new employee.</summary>
    [HttpPost]
    public async Task<ActionResult<EmployeeState>> Create([FromBody] EmployeeMetadataRequest request, CancellationToken cancellationToken)
    {
        EmployeeState created = await employeeService.CreateEmployeeAsync(request.Name, request.Role, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.EmployeeId.Value }, created);
    }

    /// <summary>PUT api/v1/ui/employees/{id}/metadata -- updates an employee's name/role.</summary>
    [HttpPut("{id:guid}/metadata")]
    public async Task<ActionResult<EmployeeState>> UpdateMetadata(Guid id, [FromBody] EmployeeMetadataRequest request, CancellationToken cancellationToken)
    {
        EmployeeState? updated = await employeeService.UpdateEmployeeMetadataAsync(EmployeeId.From(id), request.Name, request.Role, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    /// <summary>PUT api/v1/ui/employees/{id}/training -- replaces an employee's completed training codes.</summary>
    [HttpPut("{id:guid}/training")]
    public async Task<ActionResult<EmployeeState>> UpdateTraining(Guid id, [FromBody] EmployeeTrainingRequest request, CancellationToken cancellationToken)
    {
        EmployeeState? updated = await employeeService.UpdateTrainingProfileAsync(EmployeeId.From(id), request.CompletedTrainingCodes, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    /// <summary>PUT api/v1/ui/employees/{id}/certifications -- replaces an employee's certifications.</summary>
    [HttpPut("{id:guid}/certifications")]
    public async Task<ActionResult<EmployeeState>> UpdateCertifications(Guid id, [FromBody] IReadOnlyList<EmployeeCertification> certifications, CancellationToken cancellationToken)
    {
        EmployeeState? updated = await employeeService.UpdateCertificationsAsync(EmployeeId.From(id), certifications, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    /// <summary>POST api/v1/ui/employees/{id}/exposures -- records a hazard exposure.</summary>
    [HttpPost("{id:guid}/exposures")]
    public async Task<ActionResult<EmployeeState>> RecordExposure(Guid id, [FromBody] EmployeeExposureRecord record, CancellationToken cancellationToken)
    {
        EmployeeState? updated = await employeeService.RecordExposureAsync(EmployeeId.From(id), record, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    /// <summary>POST api/v1/ui/employees/{id}/injuries -- records an injury.</summary>
    [HttpPost("{id:guid}/injuries")]
    public async Task<ActionResult<EmployeeState>> RecordInjury(Guid id, [FromBody] EmployeeInjuryRequest request, CancellationToken cancellationToken)
    {
        EmployeeState? updated = await employeeService.RecordInjuryAsync(EmployeeId.From(id), request.Description, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }
}
