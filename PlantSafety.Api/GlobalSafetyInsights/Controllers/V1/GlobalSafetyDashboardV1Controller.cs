// ============================================================================
// File: GlobalSafetyDashboardV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (Phase 8 §5 Global Safety Dashboard)
// Created: 2026-08-03
// Description: V1 controller exposing the Admin/Executive Global Safety Dashboard -- the
//              Multi-Site Safety Intelligence Network's cross-site KPI/anomaly/region-heatmap/
//              incident-timeline/corrective-action-tracker read surface. Built as a thin HTTP
//              boundary over the already-existing GlobalSafetyInsights Application layer (KPI
//              snapshot + risk score + anomaly + recommendation queries, and the §3/§4 cross-site
//              coordinated-action audit trail) -- no business logic here.
// ============================================================================
//
// Onboarding Notes:
// - Routes:
//   GET api/v1/global-safety-dashboard/summary          -- KPI/history summary (GetGlobalSafetyInsightsQuery)
//   GET api/v1/global-safety-dashboard/detail            -- full detail: per-site risk scores,
//                                                            anomaly detail, recommendation detail
//                                                            (backs the anomaly-map / region-heatmap widgets)
//   GET api/v1/global-safety-dashboard/cross-site-actions -- cross-site coordinated-action audit
//                                                            trail (backs the corrective-action
//                                                            tracker / incident-timeline widgets)
// - TenantId is NEVER accepted from the client -- every query below resolves TenantId
//   server-side via RequestEnvelopeFactory.NewQueryEnvelope() (which reads IOperationContext,
//   itself populated by TenantResolutionMiddleware -- respecting any active impersonation
//   session exactly like every other Admin controller). This mirrors the platform-wide cleanup
//   that removed client-supplied TenantId from the modern Features/ tree controllers.
// - Gated at Approver/Admin tier for the entire controller per the Phase 8 §5 spec --
//   see GlobalSafetyDashboardAuthorizationPolicies.
//
// ============================================================================

using PlantSafety.Api.Common;
using PlantSafety.Api.Context;
using PlantSafety.Api.GlobalSafetyInsights.Authorization;
using PlantSafety.Application.GlobalSafetyInsights.Models;
using PlantSafety.Application.GlobalSafetyInsights.Queries;

namespace PlantSafety.Api.GlobalSafetyInsights.Controllers.V1;

/// <summary>V1 controller for the Admin/Executive Global Safety Dashboard (Phase 8 §5).</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/global-safety-dashboard")]
[Authorize(Policy = GlobalSafetyDashboardAuthorizationPolicies.Approve)]
[Tags("GlobalSafetyDashboard")]
public sealed class GlobalSafetyDashboardV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="GlobalSafetyDashboardV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields, including the server-resolved TenantId.</param>
    public GlobalSafetyDashboardV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>
    /// Retrieves the cross-site KPI snapshot summary (TRIR/LTIR/exposure/incident density) plus
    /// its computation history within [<paramref name="fromUtc"/>, <paramref name="toUtc"/>] for
    /// the caller's operating tenant.
    /// </summary>
    /// <param name="fromUtc">Inclusive UTC start of the history range. Defaults to 90 days before <paramref name="toUtc"/> when omitted.</param>
    /// <param name="toUtc">Inclusive UTC end of the history range. Defaults to now (UTC) when omitted.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(ApiResponse<GlobalSafetyInsightsQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<GlobalSafetyInsightsQueryResult>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSummaryAsync(
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var to = toUtc ?? DateTime.UtcNow;
        var from = fromUtc ?? to.AddDays(-90);

        var result = await _sender.Send(new GetGlobalSafetyInsightsQuery(env.TenantId, from, to), cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>
    /// Retrieves the full detail (per-site/region/machine-family risk scores, anomaly detail,
    /// recommendation detail) of the most recently computed snapshot for the caller's operating
    /// tenant -- backs the anomaly-map and region-heatmap widgets.
    /// </summary>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("detail")]
    [ProducesResponseType(typeof(ApiResponse<GlobalSafetyInsightsDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<GlobalSafetyInsightsDetailDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetDetailAsync(CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var result = await _sender.Send(new GetGlobalSafetyInsightsDetailQuery(env.TenantId), cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>
    /// Lists cross-site coordinated-action audit trail entries within [<paramref name="fromUtc"/>,
    /// <paramref name="toUtc"/>] for the caller's operating tenant -- backs the corrective-action
    /// tracker and incident-timeline widgets.
    /// </summary>
    /// <param name="fromUtc">Inclusive UTC start of the range. Defaults to 30 days before <paramref name="toUtc"/> when omitted.</param>
    /// <param name="toUtc">Inclusive UTC end of the range. Defaults to now (UTC) when omitted.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("cross-site-actions")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CrossSiteActionLogEntryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CrossSiteActionLogEntryDto>>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ListCrossSiteActionsAsync(
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var to = toUtc ?? DateTime.UtcNow;
        var from = fromUtc ?? to.AddDays(-30);

        var result = await _sender.Send(new GetCrossSiteActionHistoryQuery(env.TenantId, from, to), cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }
}
