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
//
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Features.EnterpriseFabric.Users.Contracts.Requests;
using PlantSafety.Api.Features.EnterpriseFabric.Users.Contracts.Responses;
using PlantSafety.Application.Features.EnterpriseFabric.Commands.Users.Create;

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
}
