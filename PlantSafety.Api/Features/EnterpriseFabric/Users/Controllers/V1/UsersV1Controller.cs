// ============================================================================
// File: UsersV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: V1 controller exposing EnterpriseFabric/Users Commands from the
//              Application-layer Features/ CQRS tree over HTTP.
// ============================================================================
//
// Onboarding Notes:
// - Routes follow /api/v1/features/enterprise-fabric/users/...
// - Command envelope fields are populated via RequestEnvelopeFactory, which reads
//   IOperationContext and ICurrentUserService for the current request.
// - CreateUserCommandHandler returns a plain Guid, not Result<T>; unhandled
//   failures propagate to the global exception middleware.
// - GetAsync (added 2026-08-03, Phase 7 §6.1 Admin Portal User Management) exposes the
//   already-implemented IUserRepository.ListAsync paging/filtering capability via
//   ListUsersQuery, which previously had no CQRS query or controller action wired to it.
//   No Deactivate/Reactivate endpoint exists: IUserRepository has no corresponding
//   repository method or stored procedure (only usp_User_Create/GetById/List exist) --
//   this is a genuine backend gap, not implemented here to avoid fabricating a command
//   with no backing persistence.
//
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Features.EnterpriseFabric.Users.Contracts.Requests;
using PlantSafety.Api.Features.EnterpriseFabric.Users.Contracts.Responses;
using PlantSafety.Application.Features.EnterpriseFabric.Commands.Users.Create;
using PlantSafety.Application.Features.EnterpriseFabric.Queries.Users.List;

namespace PlantSafety.Api.Features.EnterpriseFabric.Users.Controllers.V1;

/// <summary>V1 controller for EnterpriseFabric/Users command endpoints.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/features/enterprise-fabric/users")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Users")]
public sealed class UsersV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="UsersV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds Command/Query envelope fields from the current request context.</param>
    public UsersV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Creates a new user account within the tenant.</summary>
    /// <param name="request">User fields bound from the request body.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the new user identifier.</returns>
    /// <response code="200">User created successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CreateUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateUserCommand
        {
            CommandId = env.CommandId,
            CorrelationId = env.CorrelationId,
            CausationId = env.CausationId,
            RequestedAtUtc = env.RequestedAtUtc,
            ApiVersionToken = env.ApiVersionToken,
            TenantId = env.TenantId,
            ActorId = env.ActorId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Roles = request.Roles,
            SiteIds = request.SiteIds,
        };

        var userId = await _sender.Send(command, cancellationToken);

        var response = new CreateUserResponse(userId);

        return Ok(ApiResponse<CreateUserResponse>.Ok(response, env.CorrelationId.ToString()));
    }

    /// <summary>Returns a paged, filterable list of users within the tenant.</summary>
    /// <param name="request">Query parameters bound from the query string.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the paged user list.</returns>
    /// <response code="200">Users returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<ListUsersResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAsync(
        [FromQuery] ListUsersRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new ListUsersQuery
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
            Role = request.Role,
            SiteId = request.SiteId,
            IsActive = request.IsActive,
            OrderBy = request.OrderBy,
            SortDirection = request.SortDirection,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
        };

        var result = await _sender.Send(query, cancellationToken);

        var response = new ListUsersResponse(result.Items, result.TotalCount, result.PageNumber, result.PageSize);

        return Ok(ApiResponse<ListUsersResponse>.Ok(response, env.CorrelationId.ToString()));
    }
}
