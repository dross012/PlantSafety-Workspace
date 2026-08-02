// ============================================================================
// File: DashboardV1Controller.cs
// Project: PlantSafety.Api
// Module: Analytics.Dashboard
// Created: 2026-08-02
// Description: V1 controller exposing the Application-layer Analytics.Dashboard module's root
//              aggregate commands and queries over HTTP. Covers the full dashboard lifecycle:
//              Create -> Publish -> RefreshData -> Deprecate -> Close -> Archive. Mirrors
//              PlantSafety.Api.Compliance.Permits.Controllers.V1.PermitsV1Controller.
//
// Route note: this controller's route ("api/v{version}/analytics/dashboard") does not collide with
// the pre-existing read-only Dashboard sub-feature controllers under
// PlantSafety.Api/Analytics/Dashboard/<SubFeature>/Controllers (Executive, KPISnapshots,
// ReadModel, TrendAnalysis), which are routed under their own type-specific segments. It also does
// not collide with the unrelated top-level "Dashboards" (plural) module's controllers.
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers().
// - Envelope fields (CommandId/CorrelationId/TenantId/ActorId etc.) are populated via the shared
//   RequestEnvelopeFactory rather than hand-rolled per action.
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Analytics.Dashboard.Contracts.Requests;
using PlantSafety.Application.Analytics.Dashboard.Commands;
using PlantSafety.Application.Analytics.Dashboard.Queries;

using PlantSafety.Api.Common;

namespace PlantSafety.Api.Analytics.Dashboard.Controllers.V1;

/// <summary>V1 controller for the Analytics.Dashboard module's root aggregate commands and queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/analytics/dashboard")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Analytics.Dashboard")]
public sealed class DashboardV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="DashboardV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public DashboardV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Defines a new KPI Dashboard.</summary>
    [HttpPost]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CreateDashboardResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CreateDashboardResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateDashboardRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateDashboardCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            DashboardName: request.DashboardName,
            DashboardType: request.DashboardType,
            OwnerUserId: request.OwnerUserId ?? env.ActorId,
            RefreshIntervalSeconds: request.RefreshIntervalSeconds,
            WidgetCount: request.WidgetCount,
            RequesterUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Publishes a Dashboard for general use (Created -&gt; Active).</summary>
    [HttpPost("{dashboardId:guid}/publish")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<DashboardStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DashboardStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> PublishAsync(
        Guid dashboardId,
        [FromBody] PublishDashboardRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new PublishDashboardCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            DashboardId: dashboardId,
            PublishedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records a data refresh for a published Dashboard. Does not change lifecycle status.</summary>
    [HttpPost("{dashboardId:guid}/refresh")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<DashboardStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DashboardStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RefreshDataAsync(
        Guid dashboardId,
        [FromBody] RefreshDashboardDataRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RefreshDashboardDataCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            DashboardId: dashboardId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Deprecates a published Dashboard (Active -&gt; Completed).</summary>
    [HttpPost("{dashboardId:guid}/deprecate")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<DashboardStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DashboardStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeprecateAsync(
        Guid dashboardId,
        [FromBody] DeprecateDashboardRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new DeprecateDashboardCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            DashboardId: dashboardId,
            DeprecatedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Formally closes a deprecated Dashboard (Completed -&gt; Closed).</summary>
    [HttpPost("{dashboardId:guid}/close")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<DashboardStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DashboardStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CloseAsync(
        Guid dashboardId,
        [FromBody] CloseDashboardRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CloseDashboardCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            DashboardId: dashboardId,
            ClosedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Archives a closed Dashboard for long-term retention (Closed -&gt; Archived).</summary>
    [HttpPost("{dashboardId:guid}/archive")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<DashboardStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DashboardStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ArchiveAsync(
        Guid dashboardId,
        [FromBody] ArchiveDashboardRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ArchiveDashboardCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            DashboardId: dashboardId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single Dashboard read-model record by identity, including its current ETag.</summary>
    [HttpGet("{dashboardId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.Analytics.Dashboard.Models.DashboardItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByIdAsync(
        Guid dashboardId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetDashboardByIdQuery(env.TenantId, dashboardId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
