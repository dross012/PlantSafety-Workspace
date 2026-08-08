// ============================================================================
// File: StandardsCatalogV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-07
// Description: V1 controller exposing the Compliance.Standards ANSI/ASME/ISO consensus-
//              standards reference-data read-model (standards families, series,
//              requirements, equipment requirements, risk assessment rules). Global
//              reference data -- like Compliance.EPA/Compliance.OSHAStandards, none of
//              these routes are tenant-scoped; no TenantId is read from or required by the
//              JWT for any GET endpoint here. Route base is "api/v1/compliance/standards/
//              catalog" (the "/catalog" sub-path deliberately avoids colliding with the
//              sibling compliance-evaluation controller's "api/v1/compliance/standards"
//              base route being built by a parallel effort).
//
//              GET api/v1/compliance/standards/catalog/families
//              GET api/v1/compliance/standards/catalog/family/{familyCode}?skip=&take=
//              GET api/v1/compliance/standards/catalog/series/{familyCode}/{seriesCode}?skip=&take=
//              GET api/v1/compliance/standards/catalog/module/{moduleName}?skip=&take=
//              GET api/v1/compliance/standards/catalog/search?keyword=&familyCode=&skip=&take=
//              GET api/v1/compliance/standards/catalog/{requirementId}
//              GET api/v1/compliance/standards/catalog/equipment-requirements?equipmentType=&requirementId=
//              GET api/v1/compliance/standards/catalog/risk-assessment-rules?methodology=&requirementId=
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Common;
using PlantSafety.Api.Compliance.Standards.Authorization;
using PlantSafety.Application.Compliance.Standards.Models;
using PlantSafety.Application.Compliance.Standards.Queries;

namespace PlantSafety.Api.Compliance.Standards.Controllers.V1;

/// <summary>V1 controller for the Compliance.Standards ANSI/ASME/ISO consensus-standards reference-data read-model.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/compliance/standards/catalog")]
[Authorize(Policy = StandardsAuthorizationPolicies.Read)]
[Tags("Compliance.Standards")]
public sealed class StandardsCatalogV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="StandardsCatalogV1Controller"/>.</summary>
    public StandardsCatalogV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Retrieves every consensus-standards family (ANSI, ASME, ISO).</summary>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("families")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<StandardsFamilyDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetFamiliesAsync(CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new GetStandardsFamiliesQuery(), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves every consensus-standards requirement for a given family, paged.</summary>
    /// <param name="family">The standards family code (e.g. "ISO").</param>
    /// <param name="skip">Number of rows to skip.</param>
    /// <param name="take">Maximum rows to return (capped server-side at 500).</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("family/{family}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<StandardsRequirementDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByFamilyAsync(
        string family,
        [FromQuery] int skip,
        [FromQuery] int take,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new GetStandardsByFamilyQuery(family, skip, take), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves every consensus-standards requirement for a given family/series, paged.</summary>
    /// <param name="familyCode">The standards family code (e.g. "ISO").</param>
    /// <param name="seriesCode">The standards series code (e.g. "9001").</param>
    /// <param name="skip">Number of rows to skip.</param>
    /// <param name="take">Maximum rows to return (capped server-side at 500).</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("series/{familyCode}/{seriesCode}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<StandardsRequirementDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetBySeriesAsync(
        string familyCode,
        string seriesCode,
        [FromQuery] int skip,
        [FromQuery] int take,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new GetStandardsBySeriesQuery(familyCode, seriesCode, skip, take), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves every consensus-standards requirement mapped to a given PlantSafety module, paged.</summary>
    /// <param name="moduleName">PlantSafety module name to match (e.g. "EquipmentSafety").</param>
    /// <param name="skip">Number of rows to skip.</param>
    /// <param name="take">Maximum rows to return (capped server-side at 500).</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("module/{moduleName}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<StandardsRequirementDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByModuleAsync(
        string moduleName,
        [FromQuery] int skip,
        [FromQuery] int take,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new GetStandardsByModuleQuery(moduleName, skip, take), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Searches consensus-standards requirements by optional keyword/family filters, paged.</summary>
    /// <param name="keyword">Optional case-insensitive contains match against title/description.</param>
    /// <param name="familyCode">Optional exact standards family code filter.</param>
    /// <param name="skip">Number of rows to skip.</param>
    /// <param name="take">Maximum rows to return (capped server-side at 500).</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("search")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<StandardsRequirementDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SearchAsync(
        [FromQuery] string? keyword,
        [FromQuery] string? familyCode,
        [FromQuery] int skip,
        [FromQuery] int take,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new SearchStandardsRequirementsQuery(keyword, familyCode, skip, take), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves consensus-standards equipment requirements, optionally filtered by equipment type and/or parent requirement.</summary>
    /// <param name="equipmentType">Optional exact equipment type filter.</param>
    /// <param name="requirementId">Optional exact parent requirement filter.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("equipment-requirements")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<EquipmentRequirementDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetEquipmentRequirementsAsync(
        [FromQuery] string? equipmentType,
        [FromQuery] Guid? requirementId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new ListEquipmentRequirementsQuery(equipmentType, requirementId), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves consensus-standards risk assessment rules, optionally filtered by methodology and/or parent requirement.</summary>
    /// <param name="methodology">Optional exact risk assessment methodology filter.</param>
    /// <param name="requirementId">Optional exact parent requirement filter.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("risk-assessment-rules")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<RiskAssessmentRuleDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRiskAssessmentRulesAsync(
        [FromQuery] string? methodology,
        [FromQuery] Guid? requirementId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new ListRiskAssessmentRulesQuery(methodology, requirementId), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves a single consensus-standards requirement by its identity.</summary>
    /// <param name="requirementId">Identity of the requirement.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("{requirementId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<StandardsRequirementDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRequirementByIdAsync(Guid requirementId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new GetStandardsRequirementByIdQuery(requirementId), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
