// ============================================================================
// File: ExportsV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: V1 MVC controller exposing the Create endpoint for the Features/SafetyEvents
//              Exports command.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow
//   /api/v1/features/safety-events/exports/...
// - CreateExportJobCommand carries the full correlation-lineage envelope (§2.1 of the
//   Enterprise Architectural Rule Handbook). RequestEnvelopeFactory builds those fields from
//   the current request's IOperationContext/ICurrentUserService.
// - Export processing itself is asynchronous; this endpoint only queues the job in
//   Queued status. The ExportJobProcessor worker executes the export out-of-band.
//
// Rationale:
// - Routes all writes through MediatR ISender to the Application layer. No domain logic or
//   data access here.
//
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Features.SafetyEvents.Exports.Contracts.Requests;
using PlantSafety.Api.Features.SafetyEvents.Exports.Contracts.Responses;
using PlantSafety.Application.Features.SafetyEvents.Commands.Exports.Create;

namespace PlantSafety.Api.Features.SafetyEvents.Exports.Controllers.V1;

/// <summary>V1 controller for safety event data export command endpoints.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/features/safety-events/exports")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Exports")]
public sealed class ExportsV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="ExportsV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds Command/Query envelope fields from the current request.</param>
    public ExportsV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Queues an asynchronous export job for safety event data.</summary>
    /// <param name="request">Request body describing the export job to queue.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the new export job identifier.</returns>
    /// <response code="201">Export job queued successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CreateExportJobResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateExportJobRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateExportJobCommand
        {
            CommandId = env.CommandId,
            CorrelationId = env.CorrelationId,
            CausationId = env.CausationId,
            RequestedAtUtc = env.RequestedAtUtc,
            ApiVersionToken = env.ApiVersionToken,
            TenantId = env.TenantId,
            ActorId = env.ActorId,
            ExportType = request.ExportType,
            Format = request.Format,
            FromUtc = request.FromUtc,
            ToUtc = request.ToUtc,
            SiteId = request.SiteId,
            Status = request.Status,
            Severity = request.Severity,
            MaxRecords = request.MaxRecords,
        };

        var jobId = await _sender.Send(command, cancellationToken);

        var response = new CreateExportJobResponse(jobId);

        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<CreateExportJobResponse>.Ok(response, env.CorrelationId.ToString()));
    }
}
