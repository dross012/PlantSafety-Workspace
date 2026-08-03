// ============================================================================
// File: DocumentsV2Controller.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: V2 stub controller for Integrations.Documents. Maintains a forward-compatible route without breaking V1 consumers.
// ============================================================================
//
// Onboarding Notes:
// - V2 routes co-exist with V1 via Asp.Versioning URL-segment versioning. Intentionally minimal until a breaking change is required.
//
// Rationale:
// - Holds the V2 route contract so consumers can target v2 now and receive V1 semantics until V2 is fully implemented.
//
// Future Extension Seams:
// - Implement distinct V2 behaviour here when the Integrations.Documents contract needs a breaking change.
//
// ============================================================================

using PlantSafety.Api.Integrations.Documents.Contracts.Requests;
using PlantSafety.Api.Integrations.Documents.Contracts.Responses;
using PlantSafety.Api.Integrations.Documents.Authorization;

namespace PlantSafety.Api.Integrations.Documents.Controllers.V2;

/// <summary>V2 stub controller for Integrations.Documents. Delegates to V1 semantics until a breaking change is required.</summary>
[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/integrations/documents")]
[Authorize(Policy = DocumentsAuthorizationPolicies.Read)]
[Tags("Documents")]
public sealed class DocumentsV2Controller : ControllerBase
{
    private readonly ISender _sender;

    /// <summary>Initialises a new instance of <see cref="DocumentsV2Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    public DocumentsV2Controller(ISender sender) => _sender = sender;

    /// <summary>V2 read endpoint for Integrations.Documents. Currently returns a not-implemented stub.</summary>
    /// <param name="request">Query parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> with Integrations.Documents data.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<DocumentsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public Task<IActionResult> GetAsync(
        [FromQuery] GetDocumentsRequest request,
        CancellationToken cancellationToken)
    {
        var correlationId = HttpContext.Items[PlantSafety.Api.Middleware.CorrelationIdMiddleware.ItemsKey]?.ToString()
                            ?? string.Empty;
        IActionResult result = Ok(ApiResponse<DocumentsResponse>.Fail(
            "Integrations.Documents V2 endpoint not yet implemented - use v1.", correlationId));
        return Task.FromResult(result);
    }

    /// <summary>V2 stub for the paginated document list endpoint. Currently returns a not-implemented stub.</summary>
    /// <param name="request">Query parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> stub response.</returns>
    [HttpGet("list")]
    [ProducesResponseType(typeof(ApiResponse<DocumentsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public Task<IActionResult> GetListAsync(
        [FromQuery] ListDocumentsRequest request,
        CancellationToken cancellationToken)
    {
        var correlationId = HttpContext.Items[PlantSafety.Api.Middleware.CorrelationIdMiddleware.ItemsKey]?.ToString()
                            ?? string.Empty;
        IActionResult result = Ok(ApiResponse<DocumentsResponse>.Fail(
            "Integrations.Documents V2 list endpoint not yet implemented - use v1.", correlationId));
        return Task.FromResult(result);
    }

    /// <summary>V2 stub for the document detail-by-id endpoint. Currently returns a not-implemented stub.</summary>
    /// <param name="documentId">The document identifier.</param>
    /// <param name="tenantId">Tenant identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> stub response.</returns>
    [HttpGet("{documentId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<DocumentsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public Task<IActionResult> GetByIdAsync(
        Guid documentId,
        [FromQuery] Guid tenantId,
        CancellationToken cancellationToken)
    {
        var correlationId = HttpContext.Items[PlantSafety.Api.Middleware.CorrelationIdMiddleware.ItemsKey]?.ToString()
                            ?? string.Empty;
        IActionResult result = Ok(ApiResponse<DocumentsResponse>.Fail(
            "Integrations.Documents V2 detail endpoint not yet implemented - use v1.", correlationId));
        return Task.FromResult(result);
    }
}
