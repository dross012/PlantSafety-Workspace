// ============================================================================
// File: PermitsV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: V1 MVC controller exposing the Application-layer Features/Compliance/Permits
//              Commands and Queries over HTTP.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow
//   /api/v1/features/compliance/permits/...
// - Envelope fields (CommandId/CorrelationId/TraceId/etc.) are populated via the shared
//   RequestEnvelopeFactory rather than hand-rolled per action.
//
// Rationale:
// - Routes all reads/writes through MediatR ISender to the Application layer. No domain
//   logic or data access here.
//
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Features.Compliance.Permits.Contracts.Requests;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Features.Compliance.Commands.Permits.Approve;
using PlantSafety.Application.Features.Compliance.Commands.Permits.Create;
using PlantSafety.Application.Features.Compliance.Models;
using PlantSafety.Application.Features.Compliance.Queries.Permits.Get;
using PlantSafety.Application.Features.Compliance.Queries.Permits.List;

namespace PlantSafety.Api.Features.Compliance.Permits.Controllers.V1;

/// <summary>V1 controller for Permit to Work commands and queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/features/compliance/permits")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Permits")]
public sealed class PermitsV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="PermitsV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public PermitsV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Creates a new Permit to Work in Draft status.</summary>
    /// <param name="request">Permit fields bound from the request body.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the new PermitId.</returns>
    /// <response code="200">Permit created successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreatePermitRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreatePermitCommand
        {
            CommandId = env.CommandId,
            CorrelationId = env.CorrelationId,
            CausationId = env.CausationId,
            RequestedAtUtc = env.RequestedAtUtc,
            ApiVersionToken = env.ApiVersionToken,
            TenantId = env.TenantId,
            ActorId = env.ActorId,
            SiteId = request.SiteId,
            PermitType = request.PermitType,
            WorkDescription = request.WorkDescription,
            WorkLocation = request.WorkLocation,
            WorkLeadId = request.WorkLeadId,
            ValidFromUtc = request.ValidFromUtc,
            ValidToUtc = request.ValidToUtc
        };

        var permitId = await _sender.Send(command, cancellationToken);

        return Ok(ApiResponse<Guid>.Ok(permitId, env.CorrelationId.ToString()));
    }

    /// <summary>Approves a Permit to Work, transitioning it from PendingApproval to Active.</summary>
    /// <param name="permitId">Permit to approve.</param>
    /// <param name="request">Approval fields bound from the request body.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the approved PermitId.</returns>
    /// <response code="200">Permit approved successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost("{permitId:guid}/approve")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ApproveAsync(
        Guid permitId,
        [FromBody] ApprovePermitRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ApprovePermitCommand
        {
            CommandId = env.CommandId,
            CorrelationId = env.CorrelationId,
            CausationId = env.CausationId,
            RequestedAtUtc = env.RequestedAtUtc,
            ApiVersionToken = env.ApiVersionToken,
            TenantId = env.TenantId,
            ActorId = env.ActorId,
            ETag = request.ETag,
            PermitId = permitId,
            ApprovalNotes = request.ApprovalNotes
        };

        var approvedPermitId = await _sender.Send(command, cancellationToken);

        return Ok(ApiResponse<Guid>.Ok(approvedPermitId, env.CorrelationId.ToString()));
    }

    /// <summary>Returns a single Permit by identifier.</summary>
    /// <param name="permitId">Permit to retrieve.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the <see cref="PermitDto"/>.</returns>
    /// <response code="200">Permit found.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Permit not found.</response>
    [HttpGet("{permitId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PermitDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<PermitDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(
        Guid permitId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetPermitByIdQuery
        {
            TraceId = env.TraceId,
            SpanId = env.SpanId,
            ApiVersionToken = env.ApiVersionToken,
            RequestingUserId = env.RequestingUserId,
            RequestingUserRole = env.RequestingUserRole,
            TenantId = env.TenantId,
            CorrelationId = env.CorrelationId,
            CachePreference = env.CachePreference,
            SuppressUnauthorized = env.SuppressUnauthorized,
            StrictMode = env.StrictMode,
            PermitId = permitId
        };

        var permit = await _sender.Send(query, cancellationToken);

        if (permit is null)
            return NotFound(ApiResponse<PermitDto>.Fail("Permit not found.", env.CorrelationId.ToString()));

        return Ok(ApiResponse<PermitDto>.Ok(permit, env.CorrelationId.ToString()));
    }

    /// <summary>Returns a paginated list of Permits for the caller's tenant.</summary>
    /// <param name="request">Filters and paging bound from the query string.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the paginated result set.</returns>
    /// <response code="200">Permits returned successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<PermitDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListAsync(
        [FromQuery] ListPermitsRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope(cachePreference: CachePreference.None);

        var query = new ListPermitsQuery
        {
            TraceId = env.TraceId,
            SpanId = env.SpanId,
            ApiVersionToken = env.ApiVersionToken,
            RequestingUserId = env.RequestingUserId,
            RequestingUserRole = env.RequestingUserRole,
            TenantId = env.TenantId,
            CorrelationId = env.CorrelationId,
            CachePreference = env.CachePreference,
            SuppressUnauthorized = env.SuppressUnauthorized,
            StrictMode = env.StrictMode,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            OrderBy = request.OrderBy,
            SortDirection = request.SortDirection,
            SiteId = request.SiteId,
            PermitType = request.PermitType,
            Status = request.Status,
            ValidFromUtc = request.ValidFromUtc,
            ValidToUtc = request.ValidToUtc
        };

        var result = await _sender.Send(query, cancellationToken);

        return Ok(ApiResponse<PaginatedResult<PermitDto>>.Ok(result, env.CorrelationId.ToString()));
    }
}
