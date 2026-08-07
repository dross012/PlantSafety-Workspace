// ============================================================================
// File: NFPAStandardsV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-07
// Description: V1 controller exposing the Compliance.NFPAStandards read-model (NFPA hazard
//              profiles/recommended controls/change history, imported by
//              tools/NfpaReferenceImport). NFPA is not a government agency and its codes
//              (NFPA 70 National Electrical Code, NFPA 51B Hot Work, etc.) are voluntary
//              consensus guidance, not legally binding regulations like OSHA's CFR Title 29 --
//              every route below therefore models "hazard profiles with recommended controls",
//              never "regulatory requirements". Global reference data -- unlike almost every
//              other controller in this codebase, these routes are NOT tenant-scoped; no
//              TenantId is read from or required by the JWT. Audit-gap/dashboard endpoints
//              (which DO need tenant scoping because they cross into Auditing.AuditFinding) live
//              on the separate NfpaDashboardV1Controller, route base "api/v1/compliance/nfpa",
//              per this module's own routing convention (deliberately distinct from OSHA's
//              "nfpa-standards/audit-gaps" precedent).
//
//              GET api/v1/compliance/nfpa-standards/{codeNumber}/{sectionNumber}
//              GET api/v1/compliance/nfpa-standards?keyword=&codeNumber=&skip=&take=
//              GET api/v1/compliance/nfpa-standards/{hazardId}/controls
//              GET api/v1/compliance/nfpa-standards/modules/{moduleName}
//              GET api/v1/compliance/nfpa-standards/changes?sinceUtc=
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Common;
using PlantSafety.Api.Compliance.NFPAStandards.Authorization;
using PlantSafety.Application.Compliance.NFPAStandards.Models;
using PlantSafety.Application.Compliance.NFPAStandards.Queries;

namespace PlantSafety.Api.Compliance.NFPAStandards.Controllers.V1;

/// <summary>V1 controller for the Compliance.NFPAStandards read-model.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/compliance/nfpa-standards")]
[Authorize(Policy = NFPAStandardsAuthorizationPolicies.Read)]
[Tags("Compliance.NFPAStandards")]
public sealed class NFPAStandardsV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="NFPAStandardsV1Controller"/>.</summary>
    public NFPAStandardsV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Retrieves a single NFPA hazard profile by code number and section citation.</summary>
    /// <param name="codeNumber">NFPA code number (e.g. "70").</param>
    /// <param name="sectionNumber">Section citation (e.g. "110.26").</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("{codeNumber}/{sectionNumber}")]
    [ProducesResponseType(typeof(ApiResponse<NfpaHazardProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByCodeSectionAsync(string codeNumber, string sectionNumber, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new GetNfpaHazardProfileQuery(codeNumber, sectionNumber), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Searches NFPA hazard profiles by optional keyword/code filters, paged.</summary>
    /// <param name="keyword">Optional case-insensitive contains match against hazard name/description.</param>
    /// <param name="codeNumber">Optional exact NFPA code filter.</param>
    /// <param name="skip">Number of rows to skip.</param>
    /// <param name="take">Maximum rows to return (capped server-side at 500).</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<NfpaHazardProfileDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SearchAsync(
        [FromQuery] string? keyword,
        [FromQuery] string? codeNumber,
        [FromQuery] int skip,
        [FromQuery] int take,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new SearchNfpaHazardsQuery(keyword, codeNumber, skip, take), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves every recommended control derived for a given hazard.</summary>
    /// <param name="hazardId">Identity of the hazard profile.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("{hazardId:guid}/controls")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<NfpaControlMatrixDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetControlsForHazardAsync(Guid hazardId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new GetNfpaControlsForHazardQuery(hazardId), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>
    /// Retrieves every NFPA hazard mapped to a PlantSafety module -- answers "what NFPA hazard
    /// guidance applies to this operation?".
    /// </summary>
    /// <param name="moduleName">PlantSafety module name to match exactly (e.g. "MachineSafety").</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("modules/{moduleName}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<NfpaHazardModuleDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetHazardsByModuleAsync(string moduleName, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new GetNfpaHazardModulesQuery(moduleName), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves NFPA hazard profile changes (new/revised/rescinded) detected at or after a given UTC instant.</summary>
    /// <param name="sinceUtc">Inclusive lower bound on detection time.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("changes")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<NfpaHazardChangeDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetChangesSinceAsync([FromQuery] DateTime sinceUtc, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new GetNfpaChangesSinceQuery(sinceUtc), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
