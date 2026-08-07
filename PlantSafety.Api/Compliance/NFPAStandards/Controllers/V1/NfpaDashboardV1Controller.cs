// ============================================================================
// File: NfpaDashboardV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-07
// Description: V1 controller exposing NFPA hazard-guidance coverage gap detection and
//              "exposure by module/tenant" dashboard tiles, built on the NFPA hazard-profile
//              read-model (tools/NfpaReferenceImport) and the Auditing coverage data
//              (Auditing.AuditFindingNfpaCoverage). Route base is "api/v1/compliance/nfpa"
//              (NOT "nfpa-standards/audit-gaps" like OSHA's own controller) -- a deliberate,
//              intentional divergence from OSHAStandardsV1Controller's audit-gaps routing
//              precedent, per this module's routing spec. TenantId is resolved server-side from
//              the JWT for every endpoint except module-totals, which is global reference data
//              with no tenant dimension.
//
//              GET api/v1/compliance/nfpa/audit-gaps?siteId=&module=
//              GET api/v1/compliance/nfpa/audit-gaps/summary?siteId=&module=
//              GET api/v1/compliance/nfpa/dashboard/module-totals
//              GET api/v1/compliance/nfpa/dashboard/site-exposure?siteId=
//              GET api/v1/compliance/nfpa/dashboard/tenant-exposure
//              GET api/v1/compliance/nfpa/dashboard/top-uncovered?siteId=&take=
//              GET api/v1/compliance/nfpa/dashboard/exposure-trend?module=&fromUtc=&toUtc=&siteId=
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Common;
using PlantSafety.Api.Compliance.NFPAStandards.Authorization;
using PlantSafety.Application.Compliance.NFPAStandards.AuditGaps.Models;
using PlantSafety.Application.Compliance.NFPAStandards.AuditGaps.Queries;
using PlantSafety.Application.Compliance.NFPAStandards.Dashboards.Models;
using PlantSafety.Application.Compliance.NFPAStandards.Dashboards.Queries;

namespace PlantSafety.Api.Compliance.NFPAStandards.Controllers.V1;

/// <summary>V1 controller for NFPA coverage gap detection and exposure dashboard tiles.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/compliance/nfpa")]
[Authorize(Policy = NFPAStandardsAuthorizationPolicies.Read)]
[Tags("Compliance.NFPAStandards.Dashboards")]
public sealed class NfpaDashboardV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="NfpaDashboardV1Controller"/>.</summary>
    public NfpaDashboardV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>
    /// Lists NFPA hazard mappings with no covering AuditFinding at the site -- "show me NFPA
    /// gaps for this site/operation". TenantId is resolved server-side from the JWT, not read
    /// from the client, per this codebase's Zero-Trust tenant-isolation requirement.
    /// </summary>
    /// <param name="siteId">Site to check coverage for; omit to check any site.</param>
    /// <param name="module">Optional PlantSafety module to filter to (e.g. "MachineSafety").</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("audit-gaps")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<NfpaGapDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAuditGapsForSiteAsync([FromQuery] Guid? siteId, [FromQuery] string? module, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new GetNfpaGapsForSiteQuery(env.TenantId, siteId, module), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Aggregate coverage counts per module at a site -- the compact form of <see cref="GetAuditGapsForSiteAsync"/>, suited to a dashboard tile.</summary>
    /// <param name="siteId">Site to check coverage for; omit to check any site.</param>
    /// <param name="module">Optional PlantSafety module to filter to (e.g. "MachineSafety").</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("audit-gaps/summary")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<NfpaGapSummaryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAuditGapsSummaryForSiteAsync([FromQuery] Guid? siteId, [FromQuery] string? module, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new GetNfpaGapsSummaryForSiteQuery(env.TenantId, siteId, module), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Total/High/Critical NFPA hazard-mapping counts per module -- global reference data, no tenant dimension.</summary>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("dashboard/module-totals")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<NfpaModuleTotalDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetModuleTotalsAsync(CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new GetNfpaModuleTotalsQuery(), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Per-module coverage exposure for a single site. Records a trend snapshot per module as a side effect.</summary>
    /// <param name="siteId">Site to compute exposure for.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("dashboard/site-exposure")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<NfpaGapSummaryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSiteExposureAsync([FromQuery] Guid siteId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new GetNfpaSiteExposureQuery(env.TenantId, siteId), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Per-module coverage exposure across the whole tenant (any site). Records a tenant-wide trend snapshot per module as a side effect.</summary>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("dashboard/tenant-exposure")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<NfpaGapSummaryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTenantExposureAsync(CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new GetNfpaTenantExposureQuery(env.TenantId), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>The highest-severity uncovered NFPA hazard mappings across every module at a site, most severe first.</summary>
    /// <param name="siteId">Site to check coverage for; omit to check any site.</param>
    /// <param name="take">Maximum rows to return (defaults to 10, capped at 100).</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("dashboard/top-uncovered")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<NfpaGapDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTopUncoveredAsync([FromQuery] Guid? siteId, [FromQuery] int take, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new GetTopUncoveredNfpaHazardsQuery(env.TenantId, siteId, take == 0 ? 10 : take), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>
    /// Recorded exposure snapshots for a module between two UTC instants -- only reflects
    /// snapshots actually taken by prior site-exposure/tenant-exposure calls; there is no
    /// backfilled history before a module/site pair's first dashboard view.
    /// </summary>
    /// <param name="module">PlantSafety module to check (e.g. "MachineSafety").</param>
    /// <param name="fromUtc">Inclusive lower bound.</param>
    /// <param name="toUtc">Inclusive upper bound.</param>
    /// <param name="siteId">Site to return the trend for; omit for the tenant-wide trend.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("dashboard/exposure-trend")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<NfpaExposureSnapshotDto>>), StatusCodes.Status200OK)]
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

        var result = await _sender.Send(new GetNfpaExposureTrendQuery(env.TenantId, siteId, module, fromUtc, toUtc), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
