// ============================================================================
// File: NIOSHStandardsV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-07
// Description: V1 controller exposing the Compliance.NIOSHStandards read-model (NIOSH chemical
//              hazard profiles, respirator selection matrix, PPE guidance, and change history).
//              This is REFERENCE DATA, not a regulatory requirement -- global reference data,
//              like Compliance.OSHAStandards, so no TenantId is read from or required by the JWT
//              for any of these GET endpoints.
//
//              GET api/v1/compliance/niosh/hazards/{cas}
//              GET api/v1/compliance/niosh/hazards/search?q=&skip=&take=
//              GET api/v1/compliance/niosh/hazards/{hazardId}/respirators
//              GET api/v1/compliance/niosh/hazards/{hazardId}/ppe
//              GET api/v1/compliance/niosh/hazards/by-module/{module}
//              GET api/v1/compliance/niosh/changes?sinceUtc=
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Common;
using PlantSafety.Api.Compliance.NIOSHStandards.Authorization;
using PlantSafety.Application.Compliance.NIOSHStandards.Models;
using PlantSafety.Application.Compliance.NIOSHStandards.Queries;

namespace PlantSafety.Api.Compliance.NIOSHStandards.Controllers.V1;

/// <summary>V1 controller for the Compliance.NIOSHStandards read-model.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/compliance/niosh")]
[Authorize(Policy = NIOSHStandardsAuthorizationPolicies.Read)]
[Tags("Compliance.NIOSHStandards")]
public sealed class NIOSHStandardsV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="NIOSHStandardsV1Controller"/>.</summary>
    public NIOSHStandardsV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Retrieves a single NIOSH hazard profile by CAS Registry Number.</summary>
    /// <param name="cas">CAS Registry Number (e.g. "7647-01-0").</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("hazards/{cas}")]
    [ProducesResponseType(typeof(ApiResponse<NioshHazardProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetHazardProfileAsync(string cas, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new GetNioshHazardProfileQuery(cas), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Searches NIOSH hazard profiles by an optional keyword filter, paged.</summary>
    /// <param name="q">Optional case-insensitive contains match against chemical name/CAS number.</param>
    /// <param name="skip">Number of rows to skip.</param>
    /// <param name="take">Maximum rows to return (capped server-side at 500).</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("hazards/search")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<NioshHazardProfileDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SearchHazardsAsync(
        [FromQuery] string? q,
        [FromQuery] int skip,
        [FromQuery] int take,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new SearchNioshHazardsQuery(q, skip, take), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves every respirator recommendation for a given NIOSH hazard profile.</summary>
    /// <param name="hazardId">Identity of the hazard profile.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("hazards/{hazardId:guid}/respirators")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<NioshRespiratorMatrixDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRespiratorRecommendationsAsync(Guid hazardId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new GetRespiratorRecommendationsQuery(hazardId), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves every PPE guidance entry for a given NIOSH hazard profile.</summary>
    /// <param name="hazardId">Identity of the hazard profile.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("hazards/{hazardId:guid}/ppe")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<NioshPpeGuidanceDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPpeGuidanceAsync(Guid hazardId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new GetPpeGuidanceQuery(hazardId), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>
    /// Retrieves every NIOSH hazard profile mapped to a PlantSafety module -- answers "what
    /// chemical hazards are relevant to this operation?".
    /// </summary>
    /// <param name="module">PlantSafety module name to match exactly (e.g. "PPE", "MachineSafety").</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("hazards/by-module/{module}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<NioshHazardProfileDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetHazardsByModuleAsync(string module, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new GetNioshHazardsByModuleQuery(module), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves NIOSH hazard profile changes (new/revised/rescinded) detected at or after a given UTC instant.</summary>
    /// <param name="sinceUtc">Inclusive lower bound on detection time.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("changes")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<NioshHazardChangeDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetChangesSinceAsync([FromQuery] DateTime sinceUtc, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new GetNioshChangesSinceQuery(sinceUtc), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
