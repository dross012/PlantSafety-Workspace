// ============================================================================
// File: ManagementOfChangeV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: V1 MVC controller exposing the Application-layer
//              Features/Compliance/ManagementOfChange Commands over HTTP.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow
//   /api/v1/features/compliance/management-of-change/...
// - Envelope fields are populated via the shared RequestEnvelopeFactory.
//
// Rationale:
// - Routes all writes through MediatR ISender to the Application layer. No domain
//   logic or data access here.
//
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Features.Compliance.ManagementOfChange.Contracts.Requests;
using PlantSafety.Application.Features.Compliance.Commands.ManagementOfChange.Create;

namespace PlantSafety.Api.Features.Compliance.ManagementOfChange.Controllers.V1;

/// <summary>V1 controller for Management of Change commands.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/features/compliance/management-of-change")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("ManagementOfChange")]
public sealed class ManagementOfChangeV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="ManagementOfChangeV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public ManagementOfChangeV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Initiates a new Management of Change request in Draft status.</summary>
    /// <param name="request">MOC fields bound from the request body.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the new MocId.</returns>
    /// <response code="200">MOC created successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateMocRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateMocCommand
        {
            CommandId = env.CommandId,
            CorrelationId = env.CorrelationId,
            CausationId = env.CausationId,
            RequestedAtUtc = env.RequestedAtUtc,
            ApiVersionToken = env.ApiVersionToken,
            TenantId = env.TenantId,
            ActorId = env.ActorId,
            SiteId = request.SiteId,
            Title = request.Title,
            Description = request.Description,
            ChangeType = request.ChangeType,
            RiskLevel = request.RiskLevel,
            InitiatorId = request.InitiatorId,
            ProposedStartUtc = request.ProposedStartUtc,
            ProposedEndUtc = request.ProposedEndUtc
        };

        var mocId = await _sender.Send(command, cancellationToken);

        return Ok(ApiResponse<Guid>.Ok(mocId, env.CorrelationId.ToString()));
    }
}
