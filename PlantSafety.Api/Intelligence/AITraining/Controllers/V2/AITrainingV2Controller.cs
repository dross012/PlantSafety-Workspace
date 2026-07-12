// ============================================================================
// File: AITrainingV2Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: V2 stub controller for AITraining. Maintains a forward-compatible route without breaking V1 consumers.
// ============================================================================
//
// Onboarding Notes:
// - V2 routes co-exist with V1 via Asp.Versioning URL-segment versioning. Intentionally minimal until a breaking change is required.
//
// Rationale:
// - Holds the V2 route contract so consumers can target v2 now and receive V1 semantics until V2 is fully implemented.
//
// Future Extension Seams:
// - Implement distinct V2 behaviour here when the AITraining contract needs a breaking change.
//
// ============================================================================
using PlantSafety.Api.Intelligence.AITraining.Contracts.Requests;
using PlantSafety.Api.Intelligence.AITraining.Contracts.Responses;
using PlantSafety.Api.Intelligence.AITraining.Authorization;

namespace PlantSafety.Api.Intelligence.AITraining.Controllers.V2;

/// <summary>V2 stub controller for AITraining. Delegates to V1 semantics until a breaking change is required.</summary>
[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/intelligence/aitraining")]
[Authorize(Policy = AITrainingAuthorizationPolicies.Read)]
[Tags("AITraining")]
public sealed class AITrainingV2Controller : ControllerBase
{
    private readonly ISender _sender;

    /// <summary>Initialises a new instance of <see cref="AITrainingV2Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    public AITrainingV2Controller(ISender sender) => _sender = sender;

    /// <summary>V2 read endpoint for AITraining. Currently returns a not-implemented stub.</summary>
    /// <param name="request">Query parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> with AITraining data.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<AITrainingResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public Task<IActionResult> GetAsync(
        [FromQuery] GetAITrainingRequest request,
        CancellationToken cancellationToken)
    {
        var correlationId = HttpContext.Items[PlantSafety.Api.Middleware.CorrelationIdMiddleware.ItemsKey]?.ToString()
                            ?? string.Empty;
        IActionResult result = Ok(ApiResponse<AITrainingResponse>.Fail(
            "AITraining V2 endpoint not yet implemented - use v1.", correlationId));
        return Task.FromResult(result);
    }
}