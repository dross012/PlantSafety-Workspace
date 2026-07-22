// ============================================================================
// File: AuditEngagementsV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-21
// Description: V1 controller exposing the Auditing module's AuditEngagement commands and its
//              eight sub-component analytics queries (Efficiency, FacilityAssessment, Inspecting,
//              Operations, Procedures, QualityControl, RegulatoryCompliance, Systems) over HTTP.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow
//   /api/v1/auditing/engagements/...
// - This is a NEW Auditing module, distinct from the pre-existing PlantSafety.Api.Compliance.Auditing
//   tree; do not merge or relocate.
// - Auditing commands/queries return PlantSafety.Application.Common.Results.Result<T>, mapped to
//   ApiResponse<T> via the shared PlantSafety.Api.Common.ResultResponseExtensions.ToActionResult.
//
// ============================================================================

using PlantSafety.Api.Auditing.Contracts.Requests;
using PlantSafety.Api.Context;
using PlantSafety.Application.Auditing.AuditEngagement.Efficiency.Queries;
using PlantSafety.Application.Auditing.AuditEngagement.FacilityAssessment.Queries;
using PlantSafety.Application.Auditing.AuditEngagement.Inspecting.Queries;
using PlantSafety.Application.Auditing.AuditEngagement.Operations.Queries;
using PlantSafety.Application.Auditing.AuditEngagement.Procedures.Queries;
using PlantSafety.Application.Auditing.AuditEngagement.QualityControl.Queries;
using PlantSafety.Application.Auditing.AuditEngagement.RegulatoryCompliance.Queries;
using PlantSafety.Application.Auditing.AuditEngagement.Systems.Queries;
using PlantSafety.Application.Auditing.Commands;

using PlantSafety.Api.Common;

namespace PlantSafety.Api.Auditing.Controllers.V1;

/// <summary>V1 controller for Audit Engagement commands and analytics queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auditing/engagements")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Auditing")]
public sealed class AuditEngagementsV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="AuditEngagementsV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public AuditEngagementsV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Creates a new Audit Engagement.</summary>
    /// <response code="200">Audit engagement created successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateAuditEngagementResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CreateAuditEngagementResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<CreateAuditEngagementResult>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateAuditEngagementRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateAuditEngagementCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            OwnerId: env.ActorId,
            EngagementTitle: request.EngagementTitle,
            DisciplineType: request.DisciplineType,
            Scope: request.Scope,
            LeadAuditorId: request.LeadAuditorId,
            ScheduledAtUtc: request.ScheduledAtUtc);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Schedules an Audit Engagement.</summary>
    /// <response code="200">Audit engagement scheduled successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Audit engagement not found.</response>
    [HttpPost("{auditEngagementId:guid}/schedule")]
    [ProducesResponseType(typeof(ApiResponse<ScheduleAuditEngagementResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ScheduleAuditEngagementResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ScheduleAuditEngagementResult>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ScheduleAsync(
        Guid auditEngagementId,
        [FromBody] ScheduleAuditEngagementRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ScheduleAuditEngagementCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            OwnerId: env.ActorId,
            AuditEngagementId: auditEngagementId,
            LeadAuditorId: request.LeadAuditorId,
            ScheduledAtUtc: request.ScheduledAtUtc)
        {
            TenantId = env.TenantId
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Starts an Audit Engagement.</summary>
    /// <response code="200">Audit engagement started successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Audit engagement not found.</response>
    [HttpPost("{auditEngagementId:guid}/start")]
    [ProducesResponseType(typeof(ApiResponse<StartAuditEngagementResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<StartAuditEngagementResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<StartAuditEngagementResult>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> StartAsync(
        Guid auditEngagementId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new StartAuditEngagementCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            OwnerId: env.ActorId,
            AuditEngagementId: auditEngagementId)
        {
            TenantId = env.TenantId
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Closes an Audit Engagement.</summary>
    /// <response code="200">Audit engagement closed successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Audit engagement not found.</response>
    [HttpPost("{auditEngagementId:guid}/close")]
    [ProducesResponseType(typeof(ApiResponse<CloseAuditEngagementResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CloseAuditEngagementResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<CloseAuditEngagementResult>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CloseAsync(
        Guid auditEngagementId,
        [FromBody] CloseAuditEngagementRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CloseAuditEngagementCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            OwnerId: env.ActorId,
            AuditEngagementId: auditEngagementId,
            ComplianceScore: request.ComplianceScore,
            ComplianceRating: request.ComplianceRating)
        {
            TenantId = env.TenantId
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    // ── Sub-component analytics queries ─────────────────────────────────────

    /// <summary>Returns Efficiency analytics for Audit Engagements in a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("analytics/efficiency")]
    [ProducesResponseType(typeof(ApiResponse<EfficiencyQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetEfficiencyAsync(
        [FromQuery] AuditingAnalyticsQueryRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetEfficiencyQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns Facility Assessment analytics for Audit Engagements in a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("analytics/facility-assessment")]
    [ProducesResponseType(typeof(ApiResponse<FacilityAssessmentQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetFacilityAssessmentAsync(
        [FromQuery] AuditingAnalyticsQueryRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetFacilityAssessmentQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns Inspecting analytics for Audit Engagements in a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("analytics/inspecting")]
    [ProducesResponseType(typeof(ApiResponse<InspectingQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetInspectingAsync(
        [FromQuery] AuditingAnalyticsQueryRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetInspectingQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns Operations analytics for Audit Engagements in a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("analytics/operations")]
    [ProducesResponseType(typeof(ApiResponse<OperationsQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetOperationsAsync(
        [FromQuery] AuditingAnalyticsQueryRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetOperationsQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns Procedures analytics for Audit Engagements in a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("analytics/procedures")]
    [ProducesResponseType(typeof(ApiResponse<ProceduresQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetProceduresAsync(
        [FromQuery] AuditingAnalyticsQueryRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetProceduresQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns Quality Control analytics for Audit Engagements in a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("analytics/quality-control")]
    [ProducesResponseType(typeof(ApiResponse<QualityControlQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetQualityControlAsync(
        [FromQuery] AuditingAnalyticsQueryRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetQualityControlQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns Regulatory Compliance analytics for Audit Engagements in a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("analytics/regulatory-compliance")]
    [ProducesResponseType(typeof(ApiResponse<RegulatoryComplianceQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRegulatoryComplianceAsync(
        [FromQuery] AuditingAnalyticsQueryRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetRegulatoryComplianceQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns Systems analytics for Audit Engagements in a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("analytics/systems")]
    [ProducesResponseType(typeof(ApiResponse<SystemsQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSystemsAsync(
        [FromQuery] AuditingAnalyticsQueryRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetSystemsQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }
}
