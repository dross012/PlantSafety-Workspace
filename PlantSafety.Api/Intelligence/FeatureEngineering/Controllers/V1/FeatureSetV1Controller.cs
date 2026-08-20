// ============================================================================
// File: FeatureSetV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (Claude Code api-contract-validation-agent)
// Created: 2026-08-20
// Description: V1 MVC controller exposing the Create/AddFeature/Publish/by-id command and query
//              endpoints for the Intelligence.FeatureEngineering FeatureSet aggregate. Prior to
//              this file, the module's Application-layer commands/queries/handlers (ported in the
//              preceding repair stage) had no reachable API surface -- no /Controllers folder
//              existed under PlantSafety.Api/Intelligence/FeatureEngineering, unlike every sibling
//              Intelligence submodule (AIInvestigations, AITraining, DataLake, PredictiveSafety).
//              Mirrors PlantSafety.Api.Intelligence.DataLake.Controllers.V1
//              .DataLakeActivityV1Controller.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow
//   /api/v1/intelligence/feature-engineering/feature-sets/...
//
// Rationale:
// - Routes all reads/writes through MediatR ISender to the Application layer. No domain logic or
//   data access here.
//
// ============================================================================

using PlantSafety.Api.Intelligence.FeatureEngineering.Contracts.Requests;
using PlantSafety.Api.Intelligence.FeatureEngineering.Authorization;
using PlantSafety.Application.Intelligence.FeatureEngineering.Commands;
using PlantSafety.Application.Intelligence.FeatureEngineering.Queries;

using PlantSafety.Api.Common;
using PlantSafety.Api.Context;

namespace PlantSafety.Api.Intelligence.FeatureEngineering.Controllers.V1;

/// <summary>
/// V1 controller for the Create/AddFeature/Publish/by-id command and query endpoints for the
/// FeatureSet aggregate.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/intelligence/feature-engineering/feature-sets")]
[Authorize(Policy = FeatureEngineeringAuthorizationPolicies.Read)]
[Tags("FeatureSet")]
public sealed class FeatureSetV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="FeatureSetV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public FeatureSetV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Creates a new, unpublished feature set.</summary>
    [HttpPost]
    [Authorize(Policy = FeatureEngineeringAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<FeatureSetStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<FeatureSetStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateFeatureSetRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateFeatureSetCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            Name: request.Name,
            Version: request.Version,
            DataSourcePath: request.DataSourcePath,
            CreatedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Adds a named feature to an unpublished feature set.</summary>
    [HttpPost("{featureSetId:guid}/features")]
    [Authorize(Policy = FeatureEngineeringAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<FeatureSetStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<FeatureSetStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddFeatureAsync(
        Guid featureSetId,
        [FromBody] AddFeatureToFeatureSetRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new AddFeatureToFeatureSetCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            FeatureSetId: featureSetId,
            FeatureName: request.FeatureName,
            RequestedByUserId: env.ActorId)
        {
            ETag = request.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Publishes a feature set to the feature store. Immutable thereafter.</summary>
    [HttpPost("{featureSetId:guid}/publish")]
    [Authorize(Policy = FeatureEngineeringAuthorizationPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<FeatureSetStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<FeatureSetStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> PublishAsync(
        Guid featureSetId,
        [FromBody] PublishFeatureSetRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new PublishFeatureSetCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            FeatureSetId: featureSetId,
            PublishedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single FeatureSet read-model record by identity, including its current ETag.</summary>
    [HttpGet("{featureSetId:guid}")]
    [Authorize(Policy = FeatureEngineeringAuthorizationPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.Intelligence.FeatureEngineering.Models.FeatureSetDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByIdAsync(
        Guid featureSetId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetFeatureSetByIdQuery(env.TenantId, featureSetId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
