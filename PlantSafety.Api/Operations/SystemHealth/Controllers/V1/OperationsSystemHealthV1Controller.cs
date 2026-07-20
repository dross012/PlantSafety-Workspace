// ============================================================================
// File: OperationsSystemHealthV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-20
// Description: V1 controller for the Operations.SystemHealth unified rollup dashboard --
//   the final deliverable (Step 10/10) of the "bring C# up to the SQL Server hardening
//   standard" roadmap. Single read-only endpoint that fans out to every other
//   Operations.* module's existing queries via MediatR ISender. No domain logic or data
//   access here; no new aggregation logic here either -- all aggregation lives in
//   GetSystemHealthReportQueryHandler.
// ============================================================================
using PlantSafety.Application.Operations.SystemHealth.Models;
using PlantSafety.Application.Operations.SystemHealth.Queries;

namespace PlantSafety.Api.Operations.SystemHealth.Controllers.V1;

/// <summary>V1 controller for the Operations.SystemHealth module.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/operations/system-health")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Operations.SystemHealth")]
public sealed class OperationsSystemHealthV1Controller : ControllerBase
{
    private readonly ISender _sender;

    /// <summary>Initialises a new instance of <see cref="OperationsSystemHealthV1Controller"/>.</summary>
    public OperationsSystemHealthV1Controller(ISender sender) => _sender = sender;

    private string CorrelationId =>
        HttpContext.Items[PlantSafety.Api.Middleware.CorrelationIdMiddleware.ItemsKey]?.ToString() ?? string.Empty;

    /// <summary>
    /// Returns the unified System Health rollup report: backup health, schema health, DB
    /// health, DB performance/error-count summary, purge status, recent audit activity,
    /// feature flags, Task Scheduler job status, app version, and DB version. Every
    /// section degrades gracefully and is individually marked unavailable rather than
    /// failing the whole report if its underlying data source is unreachable.
    /// </summary>
    [HttpGet("report")]
    [ProducesResponseType(typeof(ApiResponse<SystemHealthReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SystemHealthReportDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetReportAsync(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetSystemHealthReportQuery(), cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<SystemHealthReportDto>.Fail(result.Error.Message, CorrelationId));

        return Ok(ApiResponse<SystemHealthReportDto>.Ok(result.Value, CorrelationId));
    }
}
