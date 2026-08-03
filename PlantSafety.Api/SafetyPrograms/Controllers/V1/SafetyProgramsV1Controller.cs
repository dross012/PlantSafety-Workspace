// ============================================================================
// File: SafetyProgramsV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (Claude Code modular-creation-agent)
// Created: 2026-08-03
// Description: V1 controller for the SafetyPrograms module -- a thin composition layer over
//              the existing SafetyPolicies/SafetyProcedures/SafetyCulture modules. Backs
//              PlantSafety.UI/Modules/SafetyPrograms/ApiClients/SafetyProgramsApiClient.cs's
//              `v1/safety-programs` and `v1/safety-programs/{programId}` routes, plus adds a
//              create/status-change surface for completeness with the canonical module shape
//              (Commands/Queries/Handlers/Models/Repositories/Validation/DependencyInjection).
//              See docs/Phase5/ui-gaps-scoping.md section 8 for the scoping decision.
// ============================================================================
//
// Onboarding Notes:
// - Envelope fields are populated via RequestEnvelopeFactory, mirroring the canonical
//   Features/Compliance/Permits pattern. Commands/queries are dispatched through MediatR ISender.
// - Responses use the shared ResultResponseExtensions.ToActionResult helper to translate the
//   Application layer's Result<T>/ApplicationError outcome into ApiResponse<T> + HTTP status.
// - The GET detail action composes live SafetyPolicies/SafetyProcedures/SafetyCulture activity
//   into the response -- see GetSafetyProgramDetailQueryHandler for the fan-out logic.
//
// ============================================================================

using PlantSafety.Api.Common;
using PlantSafety.Api.Context;
using PlantSafety.Api.SafetyPrograms.Contracts.Requests;
using PlantSafety.Application.SafetyPrograms.Commands;
using PlantSafety.Application.SafetyPrograms.Models;
using PlantSafety.Application.SafetyPrograms.Queries;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Api.SafetyPrograms.Controllers.V1;

/// <summary>V1 controller for the SafetyPrograms module.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/safety-programs")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("SafetyPrograms")]
public sealed class SafetyProgramsV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="SafetyProgramsV1Controller"/>.</summary>
    public SafetyProgramsV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Retrieves a paginated, optionally status-filtered page of safety programs.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="422">Request validation failed (e.g. unrecognized status).</response>
    [HttpGet]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<SafetyProgramListItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<SafetyProgramListItemDto>>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> GetProgramsAsync(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new GetSafetyProgramsQuery(env.TenantId, SiteId: null, status, pageNumber, pageSize), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves the full detail for a single safety program, including a live composed
    /// rollup of related SafetyPolicies/SafetyProcedures/SafetyCulture activity.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Program not found.</response>
    [HttpGet("{safetyProgramId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<SafetyProgramDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<SafetyProgramDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProgramAsync(Guid safetyProgramId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new GetSafetyProgramDetailQuery(env.TenantId, safetyProgramId), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Creates a new safety program.</summary>
    /// <response code="200">Program created successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="422">Request validation failed.</response>
    [HttpPost]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CreateSafetyProgramResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<CreateSafetyProgramResult>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateSafetyProgramRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = CreateSafetyProgramCommand.FromDto(
            tenantId: env.TenantId,
            siteId: request.SiteId,
            ownerId: request.OwnerId,
            name: request.Name,
            description: request.Description ?? string.Empty,
            category: request.Category,
            startDate: request.StartDate,
            endDate: request.EndDate,
            goals: request.Goals,
            correlationId: env.CorrelationId,
            causationId: env.CausationId,
            apiVersionToken: env.ApiVersionToken);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Transitions a safety program to a new lifecycle status.</summary>
    /// <response code="200">Status changed successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Program not found.</response>
    /// <response code="422">Request or state transition invalid.</response>
    [HttpPost("{safetyProgramId:guid}/status")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<ChangeSafetyProgramStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<ChangeSafetyProgramStatusResult>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ChangeSafetyProgramStatusResult>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ChangeStatusAsync(
        Guid safetyProgramId, [FromBody] ChangeSafetyProgramStatusRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ChangeSafetyProgramStatusCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.RequestedAtUtc,
            env.ApiVersionToken, env.TenantId, safetyProgramId, request.NewStatus, env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
