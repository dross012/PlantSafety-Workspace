// ============================================================================
// File: ImpersonationV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (Phase 7 §6.3 Support Tooling)
// Created: 2026-08-03
// Description: V1 controller exposing the Support.Impersonation bounded context: start/end/get a
//              real, time-boxed, audited tenant-impersonation session for the Admin Portal's
//              Support Console. Every action is gated at ImpersonationAuthorizationPolicies.Manage
//              (PlatformAdmin-and-above) -- impersonating another tenant's data is the single most
//              consequential support-tooling action in this platform.
// ============================================================================
//
// Onboarding Notes:
// - Routes:
//   POST api/v1/support/impersonation/tenants/{tenantId}   -- start a session
//   POST api/v1/support/impersonation/{sessionId}/end       -- end a session
//   GET  api/v1/support/impersonation/{sessionId}            -- get a session's current state
//
// ============================================================================

using PlantSafety.Api.Common;
using PlantSafety.Api.Context;
using PlantSafety.Api.Support.Impersonation.Authorization;
using PlantSafety.Api.Support.Impersonation.Contracts.Requests;
using PlantSafety.Application.Support.Impersonation.Commands;
using PlantSafety.Application.Support.Impersonation.Models;
using PlantSafety.Application.Support.Impersonation.Queries;

namespace PlantSafety.Api.Support.Impersonation.Controllers.V1;

/// <summary>V1 controller for the Support.Impersonation bounded context.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/support/impersonation")]
[Authorize(Policy = ImpersonationAuthorizationPolicies.Manage)]
[Tags("Support.Impersonation")]
public sealed class ImpersonationV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="ImpersonationV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public ImpersonationV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Starts a real, time-boxed, audited impersonation session of the given tenant.</summary>
    /// <param name="tenantId">Identity of the tenant to impersonate.</param>
    /// <param name="request">The mandatory business reason for the impersonation.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost("tenants/{tenantId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ImpersonationSessionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ImpersonationSessionDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> StartAsync(Guid tenantId, [FromBody] StartImpersonationRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new StartImpersonationCommand(tenantId, env.ActorId, request.Reason, env.CorrelationId);
        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Ends an active impersonation session.</summary>
    /// <param name="sessionId">Identity of the session to end.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost("{sessionId:guid}/end")]
    [ProducesResponseType(typeof(ApiResponse<ImpersonationSessionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ImpersonationSessionDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> EndAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new EndImpersonationCommand(sessionId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves a single impersonation session's current state (used by the persistent impersonation banner).</summary>
    /// <param name="sessionId">Identity of the session to retrieve.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("{sessionId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ImpersonationSessionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new GetActiveImpersonationSessionQuery(sessionId), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
