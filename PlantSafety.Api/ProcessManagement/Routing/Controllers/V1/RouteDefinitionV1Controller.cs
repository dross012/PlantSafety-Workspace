// ============================================================================
// File: RouteDefinitionV1Controller.cs
// Project: PlantSafety.Api
// Module: ProcessManagement.Routing
// Created: 2026-08-03
// Description: V1 controller exposing the Application-layer ProcessManagement.Routing module's
//              Commands and Queries over HTTP. Routes exactly match
//              PlantSafety.UI.Modules.Routing.ApiClients.RoutingApiClient's 7 GET endpoints
//              (routings list/detail/steps/versions/diff/simulation/approval), plus the reused
//              ProcessManagement lifecycle as POST actions (Create/SubmitForReview/Approve/
//              Retire/Archive/Revise) for parity with ProcessDefinitionV1Controller. Mirrors
//              PlantSafety.Api.ProcessManagement.Controllers.V1.ProcessDefinitionV1Controller.
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers().
// - Envelope fields are populated via the shared RequestEnvelopeFactory. ActorId always comes
//   from the authenticated caller (ICurrentUserService), never from client-supplied input.
// - All mutating actions use SafetyTierPolicies.Write; reads use SafetyTierPolicies.Read.
// - The Steps/Versions endpoints return the platform's canonical PaginatedResult<T> even though
//   the current UI client does not send pagination parameters for them -- Steps returns its full
//   single page; Versions defaults to a page size large enough to cover typical version counts
//   in one call. The Diff endpoint likewise returns its full (already server-computed) result set
//   as a single page.
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.ProcessManagement.Routing.Contracts.Requests;
using PlantSafety.Application.Common.Constants;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.ProcessManagement.Routing.Commands;
using PlantSafety.Application.ProcessManagement.Routing.Models;
using PlantSafety.Application.ProcessManagement.Routing.Queries;

using PlantSafety.Api.Common;

namespace PlantSafety.Api.ProcessManagement.Routing.Controllers.V1;

/// <summary>V1 controller for the ProcessManagement.Routing module's commands and queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/routing/routings")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("ProcessManagement.Routing")]
public sealed class RouteDefinitionV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="RouteDefinitionV1Controller"/>.</summary>
    public RouteDefinitionV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Returns a paginated, optionally search-filtered page of routings for the dashboard list.</summary>
    [HttpGet]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<RouteDefinitionSummaryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListAsync([FromQuery] ListRouteDefinitionsRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetRouteDefinitionsQuery(env.TenantId, request.Search, request.PageNumber, request.PageSize);
        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns the full detail/metadata for a single routing.</summary>
    [HttpGet("{routeDefinitionId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<RouteDefinitionDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByIdAsync(Guid routeDefinitionId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetRouteDefinitionByIdQuery(env.TenantId, routeDefinitionId);
        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns the ordered step list for a routing.</summary>
    [HttpGet("{routeDefinitionId:guid}/steps")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<RouteStepItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetStepsAsync(Guid routeDefinitionId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetRouteStepsQuery(env.TenantId, routeDefinitionId);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
            return this.ToActionResult(result, env.CorrelationId);

        var items = result.Value;
        var paged = new PaginatedResult<RouteStepItemDto>(items, items.Count, PaginationConstants.DefaultPageNumber, Math.Max(items.Count, PaginationConstants.DefaultPageSize));

        return Ok(ApiResponse<PaginatedResult<RouteStepItemDto>>.Ok(paged, env.CorrelationId.ToString()));
    }

    /// <summary>Returns the version history timeline for a routing.</summary>
    [HttpGet("{routeDefinitionId:guid}/versions")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<RouteVersionSummaryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetVersionHistoryAsync(Guid routeDefinitionId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetRouteVersionHistoryQuery(env.TenantId, routeDefinitionId, PaginationConstants.DefaultPageNumber, PaginationConstants.MaxPageSize);
        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns the server-computed, pre-aligned diff between two versions.</summary>
    [HttpGet("{routeDefinitionId:guid}/diff")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<RouteDiffRowDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetDiffAsync(
        Guid routeDefinitionId, [FromQuery] Guid previousVersionId, [FromQuery] Guid currentVersionId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetRouteDiffQuery(env.TenantId, routeDefinitionId, previousVersionId, currentVersionId);
        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Runs a server-side simulation for a routing (critical-path/bottleneck logic lives here, not in the UI).</summary>
    [HttpGet("{routeDefinitionId:guid}/simulation")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<RouteSimulationResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSimulationAsync(Guid routeDefinitionId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetRouteSimulationQuery(env.TenantId, routeDefinitionId);
        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns the current approval workflow state for a routing.</summary>
    [HttpGet("{routeDefinitionId:guid}/approval")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<RouteApprovalWorkflowDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetApprovalWorkflowAsync(Guid routeDefinitionId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetRouteApprovalWorkflowQuery(env.TenantId, routeDefinitionId);
        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Submits a new RouteDefinition record with its initial ordered step list.</summary>
    [HttpPost]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CreateRouteDefinitionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CreateRouteDefinitionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateRouteDefinitionRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateRouteDefinitionCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            PlantName: request.PlantName,
            Name: request.Name,
            Steps: request.Steps.Select(ToStepInput).ToList(),
            OwnerId: request.OwnerId,
            OwnerName: request.OwnerName,
            EffectiveFromUtc: request.EffectiveFromUtc,
            EffectiveToUtc: request.EffectiveToUtc,
            RequesterUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Submits a route definition for review (Draft -&gt; UnderReview).</summary>
    [HttpPost("{routeDefinitionId:guid}/submit-for-review")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<RouteDefinitionStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RouteDefinitionStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SubmitForReviewAsync(
        Guid routeDefinitionId, [FromBody] SubmitRouteDefinitionForReviewRequest? request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new SubmitRouteDefinitionForReviewCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            RouteDefinitionId: routeDefinitionId,
            SubmittedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Approves a route definition under review (UnderReview -&gt; Active).</summary>
    [HttpPost("{routeDefinitionId:guid}/approve")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<RouteDefinitionStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RouteDefinitionStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ApproveAsync(
        Guid routeDefinitionId, [FromBody] ApproveRouteDefinitionRequest? request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ApproveRouteDefinitionCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            RouteDefinitionId: routeDefinitionId,
            ApprovedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retires an active route definition (Active -&gt; Retired).</summary>
    [HttpPost("{routeDefinitionId:guid}/retire")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<RouteDefinitionStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RouteDefinitionStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RetireAsync(
        Guid routeDefinitionId, [FromBody] RetireRouteDefinitionRequest? request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RetireRouteDefinitionCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            RouteDefinitionId: routeDefinitionId,
            RetiredByUserId: env.ActorId,
            Reason: request?.Reason)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Archives a retired route definition for compliance retention (Retired -&gt; Archived).</summary>
    [HttpPost("{routeDefinitionId:guid}/archive")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<RouteDefinitionStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RouteDefinitionStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ArchiveAsync(
        Guid routeDefinitionId, [FromBody] ArchiveRouteDefinitionRequest? request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ArchiveRouteDefinitionCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            RouteDefinitionId: routeDefinitionId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Revises an active route definition in place (same-status action; replaces the ordered step list).</summary>
    [HttpPost("{routeDefinitionId:guid}/revise")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<RouteDefinitionStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RouteDefinitionStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ReviseAsync(
        Guid routeDefinitionId, [FromBody] ReviseRouteDefinitionRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ReviseRouteDefinitionCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            RouteDefinitionId: routeDefinitionId,
            Steps: request.Steps.Select(ToStepInput).ToList(),
            RevisedByUserId: env.ActorId)
        {
            ETag = request.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    private static RouteStepInput ToStepInput(RouteStepRequestItem item) => new(
        item.ExistingStepId, item.Name, item.Instructions, item.Resources, item.EstimatedDurationMinutes);
}
