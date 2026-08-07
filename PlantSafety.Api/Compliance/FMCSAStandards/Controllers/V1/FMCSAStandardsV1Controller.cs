// ============================================================================
// File: FMCSAStandardsV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-07
// Description: V1 controller exposing the Compliance.FMCSAStandards read-model (49 CFR FMCSA
//              standards/requirements/change history). Global reference data -- these routes are
//              NOT tenant-scoped; no TenantId is read from or required by the JWT. Mirrors
//              OSHAStandardsV1Controller's shape, minus the audit-gaps routes (those live on
//              FmcsaDashboardV1Controller per this submodule's canonical route layout).
//
//              GET api/v1/compliance/fmcsa-standards/{partNumber}/{sectionNumber}
//              GET api/v1/compliance/fmcsa-standards?keyword=&partNumber=&skip=&take=
//              GET api/v1/compliance/fmcsa-standards/{standardId}/requirements
//              GET api/v1/compliance/fmcsa-standards/requirements/by-module/{appliesToModule}
//              GET api/v1/compliance/fmcsa-standards/requirements/search?keyword=&skip=&take=
//              GET api/v1/compliance/fmcsa-standards/changes?sinceUtc=
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Common;
using PlantSafety.Api.Compliance.FMCSAStandards.Authorization;
using PlantSafety.Application.Compliance.FMCSAStandards.Models;
using PlantSafety.Application.Compliance.FMCSAStandards.Queries;

namespace PlantSafety.Api.Compliance.FMCSAStandards.Controllers.V1;

/// <summary>V1 controller for the Compliance.FMCSAStandards read-model.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/compliance/fmcsa-standards")]
[Authorize(Policy = FMCSAStandardsAuthorizationPolicies.Read)]
[Tags("Compliance.FMCSAStandards")]
public sealed class FMCSAStandardsV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="FMCSAStandardsV1Controller"/>.</summary>
    public FMCSAStandardsV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Retrieves a single FMCSA standard by CFR part number and section citation.</summary>
    /// <param name="partNumber">CFR part number (e.g. "395").</param>
    /// <param name="sectionNumber">Section citation (e.g. "395.8").</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("{partNumber}/{sectionNumber}")]
    [ProducesResponseType(typeof(ApiResponse<FmcsaStandardDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetBySectionAsync(string partNumber, string sectionNumber, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new GetFmcsaStandardQuery(partNumber, sectionNumber), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Searches FMCSA standards by optional keyword/part filters, paged.</summary>
    /// <param name="keyword">Optional case-insensitive contains match against title/text.</param>
    /// <param name="partNumber">Optional exact CFR part filter.</param>
    /// <param name="skip">Number of rows to skip.</param>
    /// <param name="take">Maximum rows to return (capped server-side at 500).</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<FmcsaStandardDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SearchAsync(
        [FromQuery] string? keyword,
        [FromQuery] string? partNumber,
        [FromQuery] int skip,
        [FromQuery] int take,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new SearchFmcsaStandardsQuery(keyword, partNumber, skip, take), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves every requirement derived from a given FMCSA standard.</summary>
    /// <param name="standardId">Identity of the standard.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("{standardId:guid}/requirements")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<FmcsaRequirementDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRequirementsForStandardAsync(Guid standardId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new GetFmcsaRequirementQuery(standardId), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>
    /// Retrieves every FMCSA requirement mapped to a PlantSafety module -- answers "what FMCSA
    /// requirements apply to this operation?". Returns an empty list until a separate mapping
    /// pass populates Compliance.FMCSARequirement.AppliesToModule.
    /// </summary>
    /// <param name="appliesToModule">PlantSafety module name to match exactly (e.g. "Scheduling").</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("requirements/by-module/{appliesToModule}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<FmcsaRequirementDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRequirementsByModuleAsync(string appliesToModule, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new GetFmcsaRequirementsForModuleQuery(appliesToModule), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Free-text searches FMCSA requirement text (e.g. "hours of service").</summary>
    /// <param name="keyword">Text to search for.</param>
    /// <param name="skip">Number of rows to skip.</param>
    /// <param name="take">Maximum rows to return (capped server-side at 500).</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("requirements/search")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<FmcsaRequirementDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<FmcsaRequirementDto>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SearchRequirementsAsync(
        [FromQuery] string keyword,
        [FromQuery] int skip,
        [FromQuery] int take,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new SearchFmcsaRequirementsQuery(keyword, skip, take), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves FMCSA standard changes (new/revised/rescinded) detected at or after a given UTC instant.</summary>
    /// <param name="sinceUtc">Inclusive lower bound on detection time.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("changes")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<FmcsaStandardChangeDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetChangesSinceAsync([FromQuery] DateTime sinceUtc, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new GetFmcsaChangesSinceQuery(sinceUtc), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
