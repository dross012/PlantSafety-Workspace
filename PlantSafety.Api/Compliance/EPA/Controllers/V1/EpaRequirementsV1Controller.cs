// ============================================================================
// File: EpaRequirementsV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-07
// Description: V1 controller exposing the Compliance.EPA regulatory reference-data read-model
//              (EPA programs, regulatory requirements, threshold rules, reporting
//              requirements, recordkeeping requirements). Global reference data -- like
//              Compliance.OSHAStandards, none of these routes are tenant-scoped; no TenantId
//              is read from or required by the JWT for any GET endpoint here. Uses the "Epa"
//              prefix (not bare "EPA") throughout to avoid collision with the pre-existing,
//              unrelated Compliance.Certification.EPA / Compliance.RegulatoryCalendar.
//              Deadlines.EPA controllers (EPAV1Controller in each), which this file does not
//              touch.
//
//              GET api/v1/compliance/epa/requirements/programs
//              GET api/v1/compliance/epa/requirements/program/{programCode}
//              GET api/v1/compliance/epa/requirements/module/{moduleName}
//              GET api/v1/compliance/epa/requirements/search?keyword=&programCode=&skip=&take=
//              GET api/v1/compliance/epa/requirements/{requirementId}
//              GET api/v1/compliance/epa/requirements/threshold-rules?programCode=&parameterName=
//              GET api/v1/compliance/epa/requirements/reporting-requirements?programCode=
//              GET api/v1/compliance/epa/requirements/recordkeeping-requirements?programCode=
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Common;
using PlantSafety.Api.Compliance.EPA.Authorization;
using PlantSafety.Application.Compliance.EPA.Models;
using PlantSafety.Application.Compliance.EPA.Queries;

namespace PlantSafety.Api.Compliance.EPA.Controllers.V1;

/// <summary>V1 controller for the Compliance.EPA regulatory reference-data read-model.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/compliance/epa/requirements")]
[Authorize(Policy = EpaAuthorizationPolicies.Read)]
[Tags("Compliance.EPA")]
public sealed class EpaRequirementsV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="EpaRequirementsV1Controller"/>.</summary>
    public EpaRequirementsV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Retrieves every EPA statutory program (RCRA, TSCA, CERCLA, CAA, CWA).</summary>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("programs")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<EpaProgramDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetProgramsAsync(CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new GetEpaProgramsQuery(), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves every EPA regulatory requirement for a given program, paged.</summary>
    /// <param name="programCode">The EPA program code (e.g. "RCRA").</param>
    /// <param name="skip">Number of rows to skip.</param>
    /// <param name="take">Maximum rows to return (capped server-side at 500).</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("program/{programCode}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<EpaRegulatoryRequirementDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRequirementsByProgramAsync(
        string programCode,
        [FromQuery] int skip,
        [FromQuery] int take,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new GetEpaRequirementsByProgramQuery(programCode, skip, take), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves every EPA regulatory requirement mapped to a given PlantSafety module, paged.</summary>
    /// <param name="moduleName">PlantSafety module name to match (e.g. "HazardousWasteManagement").</param>
    /// <param name="skip">Number of rows to skip.</param>
    /// <param name="take">Maximum rows to return (capped server-side at 500).</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("module/{moduleName}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<EpaRegulatoryRequirementDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRequirementsByModuleAsync(
        string moduleName,
        [FromQuery] int skip,
        [FromQuery] int take,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new GetEpaRequirementsByModuleQuery(moduleName, skip, take), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Searches EPA regulatory requirements by optional keyword/program filters, paged.</summary>
    /// <param name="keyword">Optional case-insensitive contains match against title/description/citation.</param>
    /// <param name="programCode">Optional exact EPA program code filter.</param>
    /// <param name="skip">Number of rows to skip.</param>
    /// <param name="take">Maximum rows to return (capped server-side at 500).</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("search")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<EpaRegulatoryRequirementDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SearchAsync(
        [FromQuery] string? keyword,
        [FromQuery] string? programCode,
        [FromQuery] int skip,
        [FromQuery] int take,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new SearchEpaRequirementsQuery(keyword, programCode, skip, take), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves numeric EPA threshold rules, optionally filtered by program and/or parameter name.</summary>
    /// <param name="programCode">Optional exact EPA program code filter.</param>
    /// <param name="parameterName">Optional exact parameter name filter.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("threshold-rules")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<EpaThresholdRuleDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetThresholdRulesAsync(
        [FromQuery] string? programCode,
        [FromQuery] string? parameterName,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new ListEpaThresholdRulesQuery(programCode, parameterName), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves EPA reporting requirements, optionally filtered by program.</summary>
    /// <param name="programCode">Optional exact EPA program code filter.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("reporting-requirements")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<EpaReportingRequirementDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetReportingRequirementsAsync(
        [FromQuery] string? programCode,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new ListEpaReportingRequirementsQuery(programCode), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves EPA recordkeeping requirements, optionally filtered by program.</summary>
    /// <param name="programCode">Optional exact EPA program code filter.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("recordkeeping-requirements")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<EpaRecordkeepingRequirementDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRecordkeepingRequirementsAsync(
        [FromQuery] string? programCode,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new ListEpaRecordkeepingRequirementsQuery(programCode), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves a single EPA regulatory requirement by its identity.</summary>
    /// <param name="requirementId">Identity of the requirement.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("{requirementId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<EpaRegulatoryRequirementDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRequirementByIdAsync(Guid requirementId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new GetEpaRequirementByIdQuery(requirementId), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
