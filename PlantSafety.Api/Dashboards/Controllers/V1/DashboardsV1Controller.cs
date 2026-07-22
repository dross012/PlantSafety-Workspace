// ============================================================================
// File: DashboardsV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-21
// Description: V1 MVC controller exposing the Application-layer Dashboards.Dashboard
//              Commands and Queries over HTTP.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow
//   /api/v1/dashboards/...
// - Envelope fields are populated via the shared RequestEnvelopeFactory. Dashboard commands
//   implement ICommand<Result<T>>; Dashboard queries implement plain IRequest<Result<T>>
//   (TenantId only, no full query envelope).
// - CreateDashboardCommand carries a genuine positional TenantId; the other four Dashboard
//   commands (Publish/Archive/AddWidget/RemoveWidget) carry a placeholder TenantId that
//   defaults to Guid.Empty on the record. This controller always sets TenantId = env.TenantId
//   explicitly for all five, regardless of the underlying placeholder.
// - ActorId is a read-only computed property on every Dashboard command, mapped to a
//   domain-specific "by user" field (e.g. PublishedByUserId). Since the computed property
//   itself cannot be set, this controller sets the underlying field from env.ActorId instead.
// - Widget-aggregate commands/queries live in the sibling WidgetsV1Controller in this same
//   file group (see WidgetsV1Controller.cs), also under /api/v1/widgets, per the auditing
//   module's precedent of one controller per aggregate.
//
// Rationale:
// - Routes all reads/writes through MediatR ISender to the Application layer. No domain
//   logic or data access here.
//
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Dashboards.Contracts.Requests;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Dashboards.Dashboard.Commands;
using PlantSafety.Application.Dashboards.Dashboard.Models;
using PlantSafety.Application.Dashboards.Dashboard.Queries;

using PlantSafety.Api.Common;

namespace PlantSafety.Api.Dashboards.Controllers.V1;

/// <summary>V1 controller for Dashboard lifecycle commands and queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/dashboards")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Dashboards")]
public sealed class DashboardsV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="DashboardsV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public DashboardsV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Creates a new draft Dashboard.</summary>
    /// <param name="request">Dashboard fields bound from the request body.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the <see cref="CreateDashboardResult"/>.</returns>
    /// <response code="200">Dashboard created successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateDashboardResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateDashboardRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateDashboardCommand(
            env.CorrelationId,
            env.CausationId,
            env.RequestedAtUtc,
            env.TenantId,
            request.SiteId,
            request.Name,
            request.Description,
            request.CategoryCode,
            request.DefaultFilters,
            env.ActorId)
        {
            CommandId = env.CommandId,
            ApiVersionToken = env.ApiVersionToken
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Publishes a draft Dashboard, making it visible to its permitted role(s).</summary>
    /// <param name="dashboardId">Dashboard to publish.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the <see cref="PublishDashboardResult"/>.</returns>
    /// <response code="200">Dashboard published successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost("{dashboardId:guid}/publish")]
    [ProducesResponseType(typeof(ApiResponse<PublishDashboardResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> PublishAsync(
        Guid dashboardId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new PublishDashboardCommand(
            env.CorrelationId,
            env.CausationId,
            env.RequestedAtUtc,
            dashboardId,
            env.ActorId)
        {
            CommandId = env.CommandId,
            ApiVersionToken = env.ApiVersionToken,
            TenantId = env.TenantId
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Archives a Dashboard, removing it from navigation.</summary>
    /// <param name="dashboardId">Dashboard to archive.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the <see cref="ArchiveDashboardResult"/>.</returns>
    /// <response code="200">Dashboard archived successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost("{dashboardId:guid}/archive")]
    [ProducesResponseType(typeof(ApiResponse<ArchiveDashboardResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ArchiveAsync(
        Guid dashboardId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ArchiveDashboardCommand(
            env.CorrelationId,
            env.CausationId,
            env.RequestedAtUtc,
            dashboardId,
            env.ActorId)
        {
            CommandId = env.CommandId,
            ApiVersionToken = env.ApiVersionToken,
            TenantId = env.TenantId
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Adds a Widget placement to a Dashboard's layout.</summary>
    /// <param name="dashboardId">Dashboard to modify.</param>
    /// <param name="request">Placement fields bound from the request body.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the <see cref="AddWidgetToDashboardResult"/>.</returns>
    /// <response code="200">Widget added successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost("{dashboardId:guid}/widgets")]
    [ProducesResponseType(typeof(ApiResponse<AddWidgetToDashboardResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddWidgetAsync(
        Guid dashboardId,
        [FromBody] AddWidgetToDashboardRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new AddWidgetToDashboardCommand(
            env.CorrelationId,
            env.CausationId,
            env.RequestedAtUtc,
            dashboardId,
            request.WidgetId,
            request.Row,
            request.Column,
            request.RowSpan,
            request.ColumnSpan,
            request.FilterOverrides,
            env.ActorId)
        {
            CommandId = env.CommandId,
            ApiVersionToken = env.ApiVersionToken,
            TenantId = env.TenantId
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Removes a Widget placement from a Dashboard's layout.</summary>
    /// <param name="dashboardId">Dashboard to modify.</param>
    /// <param name="widgetId">Widget placement to remove.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the <see cref="RemoveWidgetFromDashboardResult"/>.</returns>
    /// <response code="200">Widget removed successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpDelete("{dashboardId:guid}/widgets/{widgetId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<RemoveWidgetFromDashboardResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveWidgetAsync(
        Guid dashboardId,
        Guid widgetId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RemoveWidgetFromDashboardCommand(
            env.CorrelationId,
            env.CausationId,
            env.RequestedAtUtc,
            dashboardId,
            widgetId,
            env.ActorId)
        {
            CommandId = env.CommandId,
            ApiVersionToken = env.ApiVersionToken,
            TenantId = env.TenantId
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single Dashboard by identifier, including its resolved widget layout.</summary>
    /// <param name="dashboardId">Dashboard to retrieve.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the <see cref="DashboardDto"/>.</returns>
    /// <response code="200">Dashboard found.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Dashboard not found.</response>
    [HttpGet("{dashboardId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<DashboardDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<DashboardDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(
        Guid dashboardId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetDashboardByIdQuery(env.TenantId, dashboardId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns all published Dashboards visible to the given viewer role.</summary>
    /// <param name="viewerRole">Viewer role code (e.g. "Executive", "SafetyManager", "Supervisor", "MaintenanceEngineering", "ContractorCoordinator").</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the matching dashboard list.</returns>
    /// <response code="200">Dashboards returned successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<DashboardDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetForRoleAsync(
        [FromQuery] string viewerRole,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetDashboardsForRoleQuery(env.TenantId, viewerRole);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

}
