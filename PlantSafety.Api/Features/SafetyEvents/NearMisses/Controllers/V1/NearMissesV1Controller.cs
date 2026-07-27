// ============================================================================
// File: NearMissesV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: V1 MVC controller exposing the Application-layer Features/SafetyEvents
//              NearMisses Commands/Queries over HTTP.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow
//   /api/v1/features/safety-events/near-misses/...
// - Envelope fields (CommandId/CorrelationId/TraceId/etc.) are populated via
//   RequestEnvelopeFactory rather than hand-rolled from HttpContext.Items.
//
// Rationale:
// - Routes all reads/writes through MediatR ISender to the Application layer.
//   No domain logic or data access here.
//
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Features.SafetyEvents.NearMisses.Contracts.Requests;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Features.SafetyEvents.Commands.NearMisses.Create;
using PlantSafety.Application.Features.SafetyEvents.Commands.NearMisses.Update;
using PlantSafety.Application.Features.SafetyEvents.Models.NearMisses;
using PlantSafety.Application.Features.SafetyEvents.Queries.NearMisses.Get;
using PlantSafety.Application.Features.SafetyEvents.Queries.NearMisses.List;

namespace PlantSafety.Api.Features.SafetyEvents.NearMisses.Controllers.V1;

/// <summary>V1 controller for Near Miss Commands and Queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/features/safety-events/near-misses")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("SafetyEvents.NearMisses")]
public sealed class NearMissesV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="NearMissesV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds canonical Command/Query envelope values for the current request.</param>
    public NearMissesV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Creates a new Near Miss record.</summary>
    /// <param name="request">Near miss payload bound from the request body.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the new Near Miss's identifier.</returns>
    /// <response code="201">Near miss created successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateNearMissRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateNearMissCommand
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
            OccurredAtUtc = request.OccurredAtUtc,
            PotentialSeverity = request.PotentialSeverity,
            HazardCategory = request.HazardCategory,
            ReportedByWorkerId = request.ReportedByWorkerId
        };

        var nearMissId = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(
            nameof(GetByIdAsync),
            new { nearMissId },
            ApiResponse<Guid>.Ok(nearMissId, env.CorrelationId.ToString()));
    }

    /// <summary>Updates mutable fields of an existing Near Miss.</summary>
    /// <param name="nearMissId">Near miss to update.</param>
    /// <param name="request">Updated fields bound from the request body.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the updated Near Miss's identifier.</returns>
    /// <response code="200">Near miss updated successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPut("{nearMissId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateAsync(
        [FromRoute] Guid nearMissId,
        [FromBody] UpdateNearMissRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new UpdateNearMissCommand
        {
            CommandId = env.CommandId,
            CorrelationId = env.CorrelationId,
            CausationId = env.CausationId,
            RequestedAtUtc = env.RequestedAtUtc,
            ApiVersionToken = env.ApiVersionToken,
            TenantId = env.TenantId,
            ActorId = env.ActorId,
            ETag = request.ETag,
            NearMissId = nearMissId,
            Title = request.Title,
            Description = request.Description,
            PotentialSeverity = request.PotentialSeverity,
            HazardCategory = request.HazardCategory
        };

        var updatedId = await _sender.Send(command, cancellationToken);

        return Ok(ApiResponse<Guid>.Ok(updatedId, env.CorrelationId.ToString()));
    }

    /// <summary>Returns the full Near Miss projection for a given identifier.</summary>
    /// <param name="nearMissId">Near miss to retrieve.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the <see cref="NearMissDto"/>.</returns>
    /// <response code="200">Near miss found.</response>
    /// <response code="404">Near miss not found.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("{nearMissId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<NearMissDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<NearMissDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByIdAsync(
        [FromRoute] Guid nearMissId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetNearMissByIdQuery
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
            NearMissId = nearMissId
        };

        var nearMiss = await _sender.Send(query, cancellationToken);

        if (nearMiss is null)
            return NotFound(ApiResponse<NearMissDto>.Fail("Near miss not found.", env.CorrelationId.ToString()));

        return Ok(ApiResponse<NearMissDto>.Ok(nearMiss, env.CorrelationId.ToString()));
    }

    /// <summary>Returns a paginated list of Near Miss records for the caller's tenant.</summary>
    /// <param name="request">Filter and paging parameters bound from the query string.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the paginated Near Miss records.</returns>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<NearMissDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListAsync(
        [FromQuery] ListNearMissesRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new ListNearMissesQuery
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
            PotentialSeverity = request.PotentialSeverity,
            HazardCategory = request.HazardCategory,
            OccurredFromUtc = request.OccurredFromUtc,
            OccurredToUtc = request.OccurredToUtc
        };

        var result = await _sender.Send(query, cancellationToken);

        return Ok(ApiResponse<PaginatedResult<NearMissDto>>.Ok(result, env.CorrelationId.ToString()));
    }
}
