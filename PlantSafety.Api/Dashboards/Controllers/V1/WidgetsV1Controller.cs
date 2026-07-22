// ============================================================================
// File: WidgetsV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-21
// Description: V1 MVC controller exposing the Application-layer Dashboards.Widget
//              Commands and Queries over HTTP.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow
//   /api/v1/widgets/...
// - Envelope fields are populated via the shared RequestEnvelopeFactory. Widget commands
//   implement ICommand<Result<T>>; Widget queries implement plain IRequest<Result<T>>
//   (TenantId only, no full query envelope).
// - CreateWidgetCommand carries a genuine positional TenantId; the other three Widget
//   commands (Configure/Activate/Deprecate) carry a placeholder TenantId that defaults to
//   Guid.Empty on the record. This controller always sets TenantId = env.TenantId
//   explicitly for all four, regardless of the underlying placeholder.
// - ActorId is a read-only computed property on every Widget command, mapped to a
//   domain-specific "by user" field (e.g. ConfiguredByUserId). Since the computed property
//   itself cannot be set, this controller sets the underlying field from env.ActorId instead.
// - Dashboard-aggregate commands/queries live in the sibling DashboardsV1Controller in this
//   same folder, under /api/v1/dashboards, per the auditing module's precedent of one
//   controller per aggregate.
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
using PlantSafety.Application.Dashboards.Widget.Commands;
using PlantSafety.Application.Dashboards.Widget.Models;
using PlantSafety.Application.Dashboards.Widget.Queries;

using PlantSafety.Api.Common;

namespace PlantSafety.Api.Dashboards.Controllers.V1;

/// <summary>V1 controller for Widget lifecycle commands and queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/widgets")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Dashboards")]
public sealed class WidgetsV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="WidgetsV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public WidgetsV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Creates a new draft Widget definition in the reusable widget library.</summary>
    /// <param name="request">Widget fields bound from the request body.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the <see cref="CreateWidgetResult"/>.</returns>
    /// <response code="200">Widget created successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateWidgetResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateWidgetRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateWidgetCommand(
            env.CorrelationId,
            env.CausationId,
            env.RequestedAtUtc,
            env.TenantId,
            request.Name,
            request.TypeCode,
            request.SourceModule,
            request.QueryOrEndpoint,
            request.RefreshIntervalSeconds,
            request.Parameters,
            request.PermittedRoles,
            env.ActorId)
        {
            CommandId = env.CommandId,
            ApiVersionToken = env.ApiVersionToken
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Reconfigures a Widget's data source, refresh interval, and/or configuration parameters.</summary>
    /// <param name="widgetId">Widget to reconfigure.</param>
    /// <param name="request">Configuration fields bound from the request body.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the <see cref="ConfigureWidgetResult"/>.</returns>
    /// <response code="200">Widget reconfigured successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost("{widgetId:guid}/configure")]
    [ProducesResponseType(typeof(ApiResponse<ConfigureWidgetResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ConfigureAsync(
        Guid widgetId,
        [FromBody] ConfigureWidgetRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ConfigureWidgetCommand(
            env.CorrelationId,
            env.CausationId,
            env.RequestedAtUtc,
            widgetId,
            request.SourceModule,
            request.QueryOrEndpoint,
            request.RefreshIntervalSeconds,
            request.Parameters,
            env.ActorId)
        {
            CommandId = env.CommandId,
            ApiVersionToken = env.ApiVersionToken,
            TenantId = env.TenantId
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Activates a draft Widget, making it available for placement on dashboards.</summary>
    /// <param name="widgetId">Widget to activate.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the <see cref="ActivateWidgetResult"/>.</returns>
    /// <response code="200">Widget activated successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost("{widgetId:guid}/activate")]
    [ProducesResponseType(typeof(ApiResponse<ActivateWidgetResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ActivateAsync(
        Guid widgetId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ActivateWidgetCommand(
            env.CorrelationId,
            env.CausationId,
            env.RequestedAtUtc,
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

    /// <summary>Deprecates an active Widget definition.</summary>
    /// <param name="widgetId">Widget to deprecate.</param>
    /// <param name="request">Deprecation fields bound from the request body.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the <see cref="DeprecateWidgetResult"/>.</returns>
    /// <response code="200">Widget deprecated successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost("{widgetId:guid}/deprecate")]
    [ProducesResponseType(typeof(ApiResponse<DeprecateWidgetResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeprecateAsync(
        Guid widgetId,
        [FromBody] DeprecateWidgetRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new DeprecateWidgetCommand(
            env.CorrelationId,
            env.CausationId,
            env.RequestedAtUtc,
            widgetId,
            request.Reason,
            env.ActorId)
        {
            CommandId = env.CommandId,
            ApiVersionToken = env.ApiVersionToken,
            TenantId = env.TenantId
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single Widget definition by identifier.</summary>
    /// <param name="widgetId">Widget to retrieve.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the <see cref="WidgetDto"/>.</returns>
    /// <response code="200">Widget found.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Widget not found.</response>
    [HttpGet("{widgetId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<WidgetDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<WidgetDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(
        Guid widgetId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetWidgetByIdQuery(env.TenantId, widgetId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Resolves the normalized data contract (Series/Categories/Metrics) for a single widget instance.</summary>
    /// <param name="widgetId">Widget to resolve data for.</param>
    /// <param name="request">Optional filter overrides bound from the query string.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the <see cref="WidgetDataContractDto"/>.</returns>
    /// <response code="200">Widget data resolved successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Widget not found.</response>
    [HttpGet("{widgetId:guid}/data")]
    [ProducesResponseType(typeof(ApiResponse<WidgetDataContractDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<WidgetDataContractDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDataAsync(
        Guid widgetId,
        [FromQuery] GetWidgetDataRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetWidgetDataQuery(env.TenantId, widgetId, request.FilterOverrides);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns the full reusable widget library (all Active widgets) for the caller's tenant.</summary>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the matching widget list.</returns>
    /// <response code="200">Widget library returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<WidgetDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetLibraryAsync(
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetWidgetLibraryQuery(env.TenantId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

}
