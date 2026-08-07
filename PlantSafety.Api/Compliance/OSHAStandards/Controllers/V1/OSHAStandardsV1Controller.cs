// ============================================================================
// File: OSHAStandardsV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-07
// Description: V1 controller exposing the Compliance.OSHAStandards read-model (CFR Title 29
//              OSHA standards/requirements/change history, imported by
//              tools/OshaStandardsImport). Global reference data -- unlike almost every other
//              controller in this codebase, most of these routes are NOT tenant-scoped; no
//              TenantId is read from or required by the JWT for those GET endpoints. The two
//              audit-gaps endpoints below are the exception: gap detection depends on
//              Auditing.AuditFinding, which IS tenant-scoped, so those two resolve TenantId from
//              the JWT like every other controller in this codebase does.
//
//              GET api/v1/compliance/osha-standards/{partNumber}/{section}
//              GET api/v1/compliance/osha-standards?keyword=&partNumber=&status=&skip=&take=
//              GET api/v1/compliance/osha-standards/{oshaStandardId}/requirements
//              GET api/v1/compliance/osha-standards/requirements/by-module/{appliesToModule}
//              GET api/v1/compliance/osha-standards/requirements/search?keyword=
//              GET api/v1/compliance/osha-standards/changes?sinceUtc=
//              GET api/v1/compliance/osha-standards/audit-gaps?siteId=&module=
//              GET api/v1/compliance/osha-standards/audit-gaps/summary?siteId=&module=
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Common;
using PlantSafety.Api.Compliance.OSHAStandards.Authorization;
using PlantSafety.Application.Compliance.OSHAStandards.Models;
using PlantSafety.Application.Compliance.OSHAStandards.Queries;
using PlantSafety.Application.Compliance.OSHAStandards.AuditGaps.Models;
using PlantSafety.Application.Compliance.OSHAStandards.AuditGaps.Queries;

namespace PlantSafety.Api.Compliance.OSHAStandards.Controllers.V1;

/// <summary>V1 controller for the Compliance.OSHAStandards read-model.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/compliance/osha-standards")]
[Authorize(Policy = OSHAStandardsAuthorizationPolicies.Read)]
[Tags("Compliance.OSHAStandards")]
public sealed class OSHAStandardsV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="OSHAStandardsV1Controller"/>.</summary>
    public OSHAStandardsV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Retrieves a single OSHA standard by CFR part number and section citation.</summary>
    /// <param name="partNumber">CFR part number (e.g. 1910).</param>
    /// <param name="section">Section citation (e.g. "&#167; 1910.1200"). URL-encode the "&#167; " prefix.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("{partNumber:int}/{section}")]
    [ProducesResponseType(typeof(ApiResponse<OSHAStandardItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetBySectionAsync(int partNumber, string section, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new GetOshaStandardBySectionQuery(partNumber, section), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Searches OSHA standards by optional keyword/part/status filters, paged.</summary>
    /// <param name="keyword">Optional case-insensitive contains match against title/text.</param>
    /// <param name="partNumber">Optional exact CFR part filter.</param>
    /// <param name="status">Optional exact status filter ("Active"/"Reserved"/"Rescinded").</param>
    /// <param name="skip">Number of rows to skip.</param>
    /// <param name="take">Maximum rows to return (capped server-side at 500).</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<OSHAStandardItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SearchAsync(
        [FromQuery] string? keyword,
        [FromQuery] int? partNumber,
        [FromQuery] string? status,
        [FromQuery] int skip,
        [FromQuery] int take,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new SearchOshaStandardsQuery(keyword, partNumber, status, skip, take), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves every requirement derived from a given OSHA standard.</summary>
    /// <param name="oshaStandardId">Identity of the standard.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("{oshaStandardId:guid}/requirements")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<OSHARequirementItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRequirementsForStandardAsync(Guid oshaStandardId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new GetOshaRequirementsForStandardQuery(oshaStandardId), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>
    /// Retrieves every OSHA requirement mapped to a PlantSafety module -- answers "what OSHA
    /// requirements apply to this operation?". Returns an empty list until a separate mapping
    /// pass populates Compliance.OSHARequirement.AppliesToModule (import alone leaves it NULL).
    /// </summary>
    /// <param name="appliesToModule">PlantSafety module name to match exactly (e.g. "PermitToWork").</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("requirements/by-module/{appliesToModule}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<OSHARequirementItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRequirementsByModuleAsync(string appliesToModule, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new GetOshaRequirementsByModuleQuery(appliesToModule), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Free-text searches OSHA requirement source sentences (e.g. "fall protection").</summary>
    /// <param name="keyword">Text to search for.</param>
    /// <param name="skip">Number of rows to skip.</param>
    /// <param name="take">Maximum rows to return (capped server-side at 500).</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("requirements/search")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<OSHARequirementItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<OSHARequirementItemDto>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SearchRequirementsAsync(
        [FromQuery] string keyword,
        [FromQuery] int skip,
        [FromQuery] int take,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new SearchOshaRequirementsQuery(keyword, skip, take), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves OSHA standard changes (new/revised/rescinded) detected at or after a given UTC instant.</summary>
    /// <param name="sinceUtc">Inclusive lower bound on detection time.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("changes")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<OSHAStandardChangeDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetChangesSinceAsync([FromQuery] DateTime sinceUtc, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new GetOshaStandardChangesSinceQuery(sinceUtc), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>
    /// Lists OSHA requirements mapped to <paramref name="module"/> with no covering AuditFinding
    /// at the site -- "show me OSHA gaps for this site/operation". TenantId is resolved
    /// server-side from the JWT, not read from the client, per this codebase's Zero-Trust
    /// tenant-isolation requirement.
    /// </summary>
    /// <param name="siteId">Site to check coverage for.</param>
    /// <param name="module">PlantSafety module to check (e.g. "MachineSafety").</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("audit-gaps")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<OshaGapDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAuditGapsForSiteAsync([FromQuery] Guid siteId, [FromQuery] string module, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new GetOshaGapsForSiteQuery(env.TenantId, siteId, module), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Aggregate coverage counts for <paramref name="module"/> at a site -- the compact form of <see cref="GetAuditGapsForSiteAsync"/>, suited to a dashboard tile.</summary>
    /// <param name="siteId">Site to check coverage for.</param>
    /// <param name="module">PlantSafety module to check (e.g. "MachineSafety").</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("audit-gaps/summary")]
    [ProducesResponseType(typeof(ApiResponse<OshaGapSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAuditGapsSummaryForSiteAsync([FromQuery] Guid siteId, [FromQuery] string module, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new GetOshaGapsSummaryForSiteQuery(env.TenantId, siteId, module), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
