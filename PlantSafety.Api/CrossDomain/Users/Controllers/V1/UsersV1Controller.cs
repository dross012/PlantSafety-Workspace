// ============================================================================
// File: UsersV1Controller.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-08-03 -- LEGACY/UNUSED FINDING (Phase 9 §6 Full-System Stability Pass +
//   post-Phase-9 investigation): this controller and PlantSafety.Api\Features\EnterpriseFabric\
//   Users\Controllers\V1\UsersV1Controller.cs share the same class name in different namespaces.
//   Investigated and confirmed: the EnterpriseFabric one is authoritative -- it is the exact
//   route family (v1/features/enterprise-fabric/users) the real Admin Portal
//   (PlantSafety.UI\Modules\Admin\ApiClients\UserManagementApiClient.cs) calls, it backs the real
//   Create/List user commands, and it received the Phase 9 governance privilege-escalation fix.
//   THIS controller's backing GetUsersQuery has ZERO callers anywhere in the solution outside its
//   own handler (verified by repo-wide grep) -- no UI ApiClient, no test, nothing routes to
//   api/v1/cross-domain/users. It is a leftover generic "<Domain>.<Verb>" legacy read-model
//   endpoint from an earlier modernization pass (the same shape as ~140 other CrossDomain-style
//   controllers elsewhere in this codebase), not a duplicate that was ever actually reachable
//   from a real caller. Left in place deliberately (not deleted) per explicit instruction: do not
//   silently remove a route until its status is triple-confirmed against every consumer,
//   including any external API client this repo can't see. Marked [Obsolete] as an advisory,
//   non-breaking signal; DO NOT wire any new caller to this controller -- use
//   PlantSafety.Api.Features.EnterpriseFabric.Users.Controllers.V1.UsersV1Controller instead.
// Description: V1 MVC controller exposing GET endpoints for the Users read-model.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow /api/v1/cross-domain/users/...
//
// Rationale:
// - Routes all reads through MediatR ISender to the Application layer. No domain logic or data access here.
//
// Future Extension Seams:
// - Add [OutputCache(PolicyName = ...)] attributes when tenant-scoped cache policies are wired in ServiceCollectionExtensions.
//
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.CrossDomain.Users.Contracts.Requests;
using PlantSafety.Api.CrossDomain.Users.Contracts.Responses;
using PlantSafety.Api.CrossDomain.Users.Authorization;
using PlantSafety.Application.CrossDomain.Users.Queries;

namespace PlantSafety.Api.CrossDomain.Users.Controllers.V1;

/// <summary>
/// V1 controller for Users read endpoints.
/// </summary>
/// <remarks>
/// <b>Legacy/unused, confirmed 2026-08-03.</b> No real caller routes to
/// <c>api/v1/cross-domain/users</c> anywhere in this solution (verified by repo-wide grep for
/// every UI <c>ApiClient</c> and test). The authoritative Users controller backing the real Admin
/// Portal is <see cref="PlantSafety.Api.Features.EnterpriseFabric.Users.Controllers.V1.UsersV1Controller"/>
/// (route <c>api/v1/features/enterprise-fabric/users</c>). Left in place, not deleted, pending an
/// explicit decision to consolidate -- do not route any new caller here.
/// </remarks>
[Obsolete("Unused legacy read-model endpoint -- confirmed zero real callers 2026-08-03. Use PlantSafety.Api.Features.EnterpriseFabric.Users.Controllers.V1.UsersV1Controller instead. Not deleted pending explicit consolidation decision.")]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/cross-domain/users")]
[Authorize(Policy = UsersAuthorizationPolicies.Read)]
[Tags("Users")]
public sealed class UsersV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="UsersV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    public UsersV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Returns Users data for the resolved tenant.</summary>
    /// <param name="request">Query parameters bound from the query string.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the Users result set.</returns>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<UsersResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAsync(
        [FromQuery] GetUsersRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var correlationId = env.CorrelationId.ToString();

        var query  = new GetUsersQuery(env.TenantId, request.SiteId, request.From, request.To ?? DateTime.UtcNow);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<UsersResponse>.Fail(result.Error.Message, correlationId));

        var response = new UsersResponse(
            result.Value.TenantId,
            result.Value.SiteId,
            result.Value.GeneratedAt,
            result.Value.Items);

        return Ok(ApiResponse<UsersResponse>.Ok(response, correlationId));
    }
}
