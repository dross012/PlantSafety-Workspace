// ============================================================================
// File: InternalSafetyAuditV2Controller.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: V2 stub controller for InternalSafetyAudit. Maintains a forward-compatible route without breaking V1 consumers.
// ============================================================================
//
// Onboarding Notes:
// - V2 routes co-exist with V1 via Asp.Versioning URL-segment versioning. Intentionally minimal until a breaking change is required.
//
// Rationale:
// - Holds the V2 route contract so consumers can target v2 now and receive V1 semantics until V2 is fully implemented.
//
// Future Extension Seams:
// - Implement distinct V2 behaviour here when the InternalSafetyAudit contract needs a breaking change.
//
// ============================================================================

using PlantSafety.Api.Compliance.Auditing.InternalSafetyAudit.Contracts.Requests;
using PlantSafety.Api.Compliance.Auditing.InternalSafetyAudit.Contracts.Responses;
using PlantSafety.Api.Compliance.Auditing.InternalSafetyAudit.Authorization;

namespace PlantSafety.Api.Compliance.Auditing.InternalSafetyAudit.Controllers.V2;

/// <summary>V2 stub controller for InternalSafetyAudit. Delegates to V1 semantics until a breaking change is required.</summary>
[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/compliance/auditing/internalsafetyaudit")]
[Authorize(Policy = InternalSafetyAuditAuthorizationPolicies.Read)]
[Tags("InternalSafetyAudit")]
public sealed class InternalSafetyAuditV2Controller : ControllerBase
{
    private readonly ISender _sender;

    /// <summary>Initialises a new instance of <see cref="InternalSafetyAuditV2Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    public InternalSafetyAuditV2Controller(ISender sender) => _sender = sender;

    /// <summary>V2 read endpoint for InternalSafetyAudit. Currently returns a not-implemented stub.</summary>
    /// <param name="request">Query parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> with InternalSafetyAudit data.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<InternalSafetyAuditResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public Task<IActionResult> GetAsync(
        [FromQuery] GetInternalSafetyAuditRequest request,
        CancellationToken cancellationToken)
    {
        var correlationId = HttpContext.Items[PlantSafety.Api.Middleware.CorrelationIdMiddleware.ItemsKey]?.ToString()
                            ?? string.Empty;
        IActionResult result = Ok(ApiResponse<InternalSafetyAuditResponse>.Fail(
            "InternalSafetyAudit V2 endpoint not yet implemented - use v1.", correlationId));
        return Task.FromResult(result);
    }
}