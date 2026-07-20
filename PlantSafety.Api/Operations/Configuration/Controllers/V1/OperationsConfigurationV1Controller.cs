// ============================================================================
// File: OperationsConfigurationV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-20
// Description: V1 controller surfacing the real Phase 14 Governance
//   [System].[FeatureFlag] tables (144-feature-flags.sql), confirmed to exist ONLY in
//   PlantSafety.ArchiveDB and PlantSafety.IntegrationDB. Routes all requests through
//   MediatR ISender to the Application layer. No domain logic or data access here.
//
//   SCOPE NOTE: This is the Configuration Framework's feature-flag editor only.
//   "Thresholds", "Routing Rules", and "Tenant Overrides" have NO backing SQL Server
//   tables in this codebase as of this build (2026-07-20) — see
//   IFeatureFlagRepository's header comment and the UI dashboard page for the full
//   scope rationale. No endpoints for those areas exist here; do not add fabricated
//   endpoints until real tables are built.
// ============================================================================
using PlantSafety.Api.Operations.Configuration.Contracts.Requests;
using PlantSafety.Application.Operations.Configuration.Commands;
using PlantSafety.Application.Operations.Configuration.Models;
using PlantSafety.Application.Operations.Configuration.Queries;

namespace PlantSafety.Api.Operations.Configuration.Controllers.V1;

/// <summary>V1 controller for the Operations.Configuration module (feature-flag editor).</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/operations/configuration")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Operations.Configuration")]
public sealed class OperationsConfigurationV1Controller : ControllerBase
{
    private readonly ISender _sender;

    /// <summary>Initialises a new instance of <see cref="OperationsConfigurationV1Controller"/>.</summary>
    public OperationsConfigurationV1Controller(ISender sender) => _sender = sender;

    private string CorrelationId =>
        HttpContext.Items[PlantSafety.Api.Middleware.CorrelationIdMiddleware.ItemsKey]?.ToString() ?? string.Empty;

    /// <summary>
    /// Returns every feature flag. When <paramref name="databaseKey"/> is omitted, returns
    /// flags from every known database (currently ArchiveDB and IntegrationDB) concatenated.
    /// </summary>
    [HttpGet("feature-flags")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<FeatureFlagDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<FeatureFlagDto>>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetFeatureFlagsAsync([FromQuery] string? databaseKey, CancellationToken cancellationToken)
    {
        var result = string.IsNullOrWhiteSpace(databaseKey)
            ? await _sender.Send(new GetAllFeatureFlagsQuery(), cancellationToken)
            : await _sender.Send(new GetFeatureFlagsQuery(databaseKey), cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<IReadOnlyList<FeatureFlagDto>>.Fail(result.Error.Message, CorrelationId));

        return Ok(ApiResponse<IReadOnlyList<FeatureFlagDto>>.Ok(result.Value, CorrelationId));
    }

    /// <summary>
    /// Toggles a single feature flag's IsEnabled value in the given database. A normal,
    /// reversible UPDATE. Uses PUT (not PATCH) to match the existing SafetyApiClientBase
    /// UI infrastructure, which provides GetAsync/PostAsync/PutAsync helpers only.
    /// </summary>
    [HttpPut("{databaseKey}/feature-flags/{flagName}")]
    [ProducesResponseType(typeof(ApiResponse<FeatureFlagDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<FeatureFlagDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetFeatureFlagEnabledAsync(
        [FromRoute] string databaseKey,
        [FromRoute] string flagName,
        [FromBody] SetFeatureFlagEnabledRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new SetFeatureFlagEnabledCommand(databaseKey, flagName, request.IsEnabled), cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<FeatureFlagDto>.Fail(result.Error.Message, CorrelationId));

        return Ok(ApiResponse<FeatureFlagDto>.Ok(result.Value, CorrelationId));
    }
}
