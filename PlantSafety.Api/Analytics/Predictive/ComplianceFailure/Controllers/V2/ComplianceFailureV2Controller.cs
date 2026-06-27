// ============================================================================
// File: ComplianceFailureV2Controller.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: V2 stub controller for ComplianceFailure. Maintains a forward-compatible route without breaking V1 consumers.
// ============================================================================
//
// Onboarding Notes:
// - V2 routes co-exist with V1 via Asp.Versioning URL-segment versioning. Intentionally minimal until a breaking change is required.
//
// Rationale:
// - Holds the V2 route contract so consumers can target v2 now and receive V1 semantics until V2 is fully implemented.
//
// Future Extension Seams:
// - Implement distinct V2 behaviour here when the ComplianceFailure contract needs a breaking change.
//
// ============================================================================

using PlantSafety.Api.Analytics.Predictive.ComplianceFailure.Contracts.Requests;
using PlantSafety.Api.Analytics.Predictive.ComplianceFailure.Contracts.Responses;
using PlantSafety.Api.Analytics.Predictive.ComplianceFailure.Authorization;

namespace PlantSafety.Api.Analytics.Predictive.ComplianceFailure.Controllers.V2;

/// <summary>V2 stub controller for ComplianceFailure. Delegates to V1 semantics until a breaking change is required.</summary>
[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/analytics/predictive/compliancefailure")]
[Authorize(Policy = ComplianceFailureAuthorizationPolicies.Read)]
[Tags("ComplianceFailure")]
public sealed class ComplianceFailureV2Controller : ControllerBase
{
    private readonly ISender _sender;

    /// <summary>Initialises a new instance of <see cref="ComplianceFailureV2Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    public ComplianceFailureV2Controller(ISender sender) => _sender = sender;

    /// <summary>V2 read endpoint for ComplianceFailure. Currently returns a not-implemented stub.</summary>
    /// <param name="request">Query parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> with ComplianceFailure data.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<ComplianceFailureResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public Task<IActionResult> GetAsync(
        [FromQuery] GetComplianceFailureRequest request,
        CancellationToken cancellationToken)
    {
        var correlationId = HttpContext.Items[PlantSafety.Api.Middleware.CorrelationIdMiddleware.ItemsKey]?.ToString()
                            ?? string.Empty;
        IActionResult result = Ok(ApiResponse<ComplianceFailureResponse>.Fail(
            "ComplianceFailure V2 endpoint not yet implemented - use v1.", correlationId));
        return Task.FromResult(result);
    }
}