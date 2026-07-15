// ============================================================================
// File: AiGovernanceV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-15
// Description: V1 controller exposing AI Governance Commands and Queries from the
//              Application-layer Features/ CQRS tree over HTTP.
// ============================================================================
//
// Onboarding Notes:
// - Routes follow /api/v1/ai-governance/models (matches the route already
//   coded into PlantSafety.UI's AiGovernanceApiClient).
//
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Features.AiGovernance.Contracts.Requests;
using PlantSafety.Api.Features.AiGovernance.Contracts.Responses;
using PlantSafety.Application.Features.AiGovernance.Commands.RegisterModel;
using PlantSafety.Application.Features.AiGovernance.Models;
using PlantSafety.Application.Features.AiGovernance.Queries.GetModel;
using PlantSafety.Application.Features.AiGovernance.Queries.ListModels;

namespace PlantSafety.Api.Features.AiGovernance.Controllers.V1;

/// <summary>V1 controller for AI Governance command and query endpoints.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/ai-governance/models")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("AiGovernance")]
public sealed class AiGovernanceV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="AiGovernanceV1Controller"/>.</summary>
    public AiGovernanceV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Registers a new governed AI/ML model.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<RegisterGovernedModelResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RegisterAsync(
        [FromBody] RegisterGovernedModelRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RegisterGovernedModelCommand
        {
            CommandId = env.CommandId,
            CorrelationId = env.CorrelationId,
            CausationId = env.CausationId,
            RequestedAtUtc = env.RequestedAtUtc,
            ApiVersionToken = env.ApiVersionToken,
            TenantId = env.TenantId,
            ActorId = env.ActorId,
            SiteId = request.SiteId,
            ModelName = request.ModelName,
            ModelVersion = request.ModelVersion,
            RiskTier = request.RiskTier,
        };

        var modelId = await _sender.Send(command, cancellationToken);

        return Ok(ApiResponse<RegisterGovernedModelResponse>.Ok(
            new RegisterGovernedModelResponse(modelId), env.CorrelationId.ToString()));
    }

    /// <summary>Returns a single governed model by identifier.</summary>
    [HttpGet("{modelId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<GovernedModelResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(Guid modelId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetGovernedModelByIdQuery
        {
            TraceId = env.TraceId,
            SpanId = env.SpanId,
            ApiVersionToken = env.ApiVersionToken,
            RequestingUserId = env.RequestingUserId,
            RequestingUserRole = env.RequestingUserRole,
            TenantId = env.TenantId,
            CorrelationId = env.CorrelationId,
            CachePreference = env.CachePreference,
            SuppressUnauthorized = env.SuppressUnauthorized,
            StrictMode = env.StrictMode,
            ModelId = modelId,
        };

        var model = await _sender.Send(query, cancellationToken);

        if (model is null)
            return NotFound(ApiResponse<GovernedModelResponse>.Fail("Governed model not found.", env.CorrelationId.ToString()));

        return Ok(ApiResponse<GovernedModelResponse>.Ok(ToResponse(model), env.CorrelationId.ToString()));
    }

    /// <summary>Returns the governed AI/ML model registry for the tenant.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<AiGovernanceListResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListAsync([FromQuery] Guid? siteId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new ListGovernedModelsQuery
        {
            TraceId = env.TraceId,
            SpanId = env.SpanId,
            ApiVersionToken = env.ApiVersionToken,
            RequestingUserId = env.RequestingUserId,
            RequestingUserRole = env.RequestingUserRole,
            TenantId = env.TenantId,
            CorrelationId = env.CorrelationId,
            CachePreference = env.CachePreference,
            SuppressUnauthorized = env.SuppressUnauthorized,
            StrictMode = env.StrictMode,
            SiteId = siteId,
        };

        var items = await _sender.Send(query, cancellationToken);

        var response = new AiGovernanceListResponse(env.TenantId, siteId, DateTime.UtcNow, items.Select(ToResponse).ToList());

        return Ok(ApiResponse<AiGovernanceListResponse>.Ok(response, env.CorrelationId.ToString()));
    }

    private static GovernedModelResponse ToResponse(GovernedModelDto dto) => new(
        dto.ModelId,
        dto.TenantId,
        dto.SiteId,
        dto.ModelName,
        dto.ModelVersion,
        dto.GovernanceStatus,
        dto.RiskTier,
        dto.DriftStatus,
        dto.LastReviewedAtUtc,
        dto.ReviewerName);
}
