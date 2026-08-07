// ============================================================================
// File: OSHADashboardV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-07
// Description: V1 controller exposing "regulatory exposure by module/tenant" dashboard tiles,
//              built on the OSHA standards/requirements read-model (tools/OshaStandardsImport)
//              and the Auditing coverage data (Auditing.AuditFindingOshaCoverage). TenantId is
//              resolved server-side from the JWT for every endpoint except module-totals, which
//              is global reference data with no tenant dimension.
//
//              GET api/v1/compliance/osha-standards/dashboard/module-totals
//              GET api/v1/compliance/osha-standards/dashboard/site-exposure?siteId=
//              GET api/v1/compliance/osha-standards/dashboard/tenant-exposure
//              GET api/v1/compliance/osha-standards/dashboard/top-uncovered?siteId=&take=
//              GET api/v1/compliance/osha-standards/dashboard/exposure-trend?module=&fromUtc=&toUtc=&siteId=
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Common;
using PlantSafety.Api.Compliance.OSHAStandards.Authorization;
using PlantSafety.Application.Compliance.OSHAStandards.AuditGaps.Models;
using PlantSafety.Application.Compliance.OSHAStandards.Dashboards.Models;
using PlantSafety.Application.Compliance.OSHAStandards.Dashboards.Queries;

namespace PlantSafety.Api.Compliance.OSHAStandards.Controllers.V1;

/// <summary>V1 controller for OSHA regulatory-exposure dashboard tiles.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/compliance/osha-standards/dashboard")]
[Authorize(Policy = OSHAStandardsAuthorizationPolicies.Read)]
[Tags("Compliance.OSHAStandards.Dashboards")]
public sealed class OSHADashboardV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="OSHADashboardV1Controller"/>.</summary>
    public OSHADashboardV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Total/High/Critical OSHA requirement counts per module -- global reference data, no tenant dimension.</summary>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("module-totals")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<OshaModuleTotalDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetModuleTotalsAsync(CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new GetOshaModuleTotalsQuery(), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Per-module coverage exposure for a single site. Records a trend snapshot per module as a side effect.</summary>
    /// <param name="siteId">Site to compute exposure for.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("site-exposure")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<OshaGapSummaryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSiteExposureAsync([FromQuery] Guid siteId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new GetOshaSiteExposureQuery(env.TenantId, siteId), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Per-module coverage exposure across the whole tenant (any site). Records a tenant-wide trend snapshot per module as a side effect.</summary>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("tenant-exposure")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<OshaGapSummaryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTenantExposureAsync(CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new GetOshaTenantExposureQuery(env.TenantId), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>The highest-severity uncovered requirements across every module at a site, most severe first.</summary>
    /// <param name="siteId">Site to check coverage for.</param>
    /// <param name="take">Maximum rows to return (defaults to 10, capped at 100).</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("top-uncovered")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<OshaGapDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTopUncoveredAsync([FromQuery] Guid siteId, [FromQuery] int take, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new GetTopUncoveredOshaRequirementsQuery(env.TenantId, siteId, take == 0 ? 10 : take), cancellationToken);

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
    [HttpGet("exposure-trend")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<OshaExposureSnapshotDto>>), StatusCodes.Status200OK)]
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

        var result = await _sender.Send(new GetOshaExposureTrendQuery(env.TenantId, siteId, module, fromUtc, toUtc), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
