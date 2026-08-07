// ============================================================================
// File: FmcsaDashboardV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-07
// Description: V1 controller exposing FMCSA audit-gap detection and "regulatory exposure by
//              module/tenant" dashboard tiles, built on the FMCSA standards/requirements
//              read-model and the Auditing coverage data (Auditing.AuditFindingFmcsaCoverage).
//              TenantId is resolved server-side from the JWT for every endpoint except
//              module-totals, which is global reference data with no tenant dimension. Mirrors
//              OSHAStandardsV1Controller/OSHADashboardV1Controller's combined shape, per this
//              submodule's canonical route layout.
//
//              GET api/v1/compliance/fmcsa/audit-gaps?siteId=&module=
//              GET api/v1/compliance/fmcsa/audit-gaps/summary?siteId=&module=
//              GET api/v1/compliance/fmcsa/dashboard/module-totals
//              GET api/v1/compliance/fmcsa/dashboard/site-exposure?siteId=
//              GET api/v1/compliance/fmcsa/dashboard/tenant-exposure
//              GET api/v1/compliance/fmcsa/dashboard/top-uncovered?siteId=&take=
//              GET api/v1/compliance/fmcsa/dashboard/exposure-trend?module=&fromUtc=&toUtc=&siteId=
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Common;
using PlantSafety.Api.Compliance.FMCSAStandards.Authorization;
using PlantSafety.Application.Compliance.FMCSAStandards.AuditGaps.Models;
using PlantSafety.Application.Compliance.FMCSAStandards.AuditGaps.Queries;
using PlantSafety.Application.Compliance.FMCSAStandards.Dashboards.Models;
using PlantSafety.Application.Compliance.FMCSAStandards.Dashboards.Queries;

namespace PlantSafety.Api.Compliance.FMCSAStandards.Controllers.V1;

/// <summary>V1 controller for FMCSA audit-gap detection and regulatory-exposure dashboard tiles.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/compliance/fmcsa")]
[Authorize(Policy = FMCSAStandardsAuthorizationPolicies.Read)]
[Tags("Compliance.FMCSAStandards.Dashboards")]
public sealed class FmcsaDashboardV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="FmcsaDashboardV1Controller"/>.</summary>
    public FmcsaDashboardV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>
    /// Lists FMCSA requirements mapped to <paramref name="module"/> (or every mapped module) with
    /// no covering AuditFinding at the site. TenantId is resolved server-side from the JWT.
    /// </summary>
    /// <param name="siteId">Site to check coverage for; omit for any site in the tenant.</param>
    /// <param name="module">PlantSafety module to check (e.g. "Scheduling"); omit for every mapped module.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("audit-gaps")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<FmcsaGapDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAuditGapsForSiteAsync([FromQuery] Guid? siteId, [FromQuery] string? module, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new GetFmcsaGapsForSiteQuery(env.TenantId, siteId, module), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Aggregate coverage counts per module (or the single module) at a site -- the compact form of <see cref="GetAuditGapsForSiteAsync"/>.</summary>
    /// <param name="siteId">Site to check coverage for; omit for any site in the tenant.</param>
    /// <param name="module">PlantSafety module to check; omit for every mapped module.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("audit-gaps/summary")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<FmcsaGapSummaryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAuditGapsSummaryForSiteAsync([FromQuery] Guid? siteId, [FromQuery] string? module, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new GetFmcsaGapsSummaryForSiteQuery(env.TenantId, siteId, module), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Total/High/Critical FMCSA requirement counts per module -- global reference data, no tenant dimension.</summary>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("dashboard/module-totals")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<FmcsaModuleTotalDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetModuleTotalsAsync(CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new GetFmcsaModuleTotalsQuery(), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Per-module coverage exposure for a single site. Records a trend snapshot per module as a side effect.</summary>
    /// <param name="siteId">Site to compute exposure for.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("dashboard/site-exposure")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<FmcsaGapSummaryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSiteExposureAsync([FromQuery] Guid siteId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new GetFmcsaSiteExposureQuery(env.TenantId, siteId), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Per-module coverage exposure across the whole tenant (any site). Records a tenant-wide trend snapshot per module as a side effect.</summary>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("dashboard/tenant-exposure")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<FmcsaGapSummaryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTenantExposureAsync(CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new GetFmcsaTenantExposureQuery(env.TenantId), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>The highest-severity uncovered requirements across every module at a site, most severe first.</summary>
    /// <param name="siteId">Site to check coverage for; omit for any site in the tenant.</param>
    /// <param name="take">Maximum rows to return (defaults to 10, capped at 100).</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("dashboard/top-uncovered")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<FmcsaGapDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTopUncoveredAsync([FromQuery] Guid? siteId, [FromQuery] int take, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new GetTopUncoveredFmcsaRequirementsQuery(env.TenantId, siteId, take == 0 ? 10 : take), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>
    /// Recorded exposure snapshots for a module between two UTC instants -- only reflects
    /// snapshots actually taken by prior site-exposure/tenant-exposure calls; there is no
    /// backfilled history before a module/site pair's first dashboard view.
    /// </summary>
    /// <param name="module">PlantSafety module to check (e.g. "Scheduling").</param>
    /// <param name="fromUtc">Inclusive lower bound.</param>
    /// <param name="toUtc">Inclusive upper bound.</param>
    /// <param name="siteId">Site to return the trend for; omit for the tenant-wide trend.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("dashboard/exposure-trend")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<FmcsaExposureSnapshotDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetExposureTrendAsync(
        [FromQuery] string module,
        [FromQuery] DateTime fromUtc,
        [FromQuery] DateTime toUtc,
        [FromQuery] Guid? siteId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new GetFmcsaExposureTrendQuery(env.TenantId, siteId, module, fromUtc, toUtc), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
