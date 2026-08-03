// ============================================================================
// File: DocumentsV1Controller.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: V1 MVC controller exposing GET endpoints for the Integrations.Documents read-model.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow /api/v1/integrations/documents/...
//
// Rationale:
// - Routes all reads through MediatR ISender to the Application layer. No domain logic or data access here.
//
// Future Extension Seams:
// - Add [OutputCache(PolicyName = ...)] attributes when tenant-scoped cache policies are wired in ServiceCollectionExtensions.
//
// ============================================================================

using PlantSafety.Api.Integrations.Documents.Contracts.Requests;
using PlantSafety.Api.Integrations.Documents.Contracts.Responses;
using PlantSafety.Api.Integrations.Documents.Authorization;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Integrations.Documents.Models;
using PlantSafety.Application.Integrations.Documents.Queries;

namespace PlantSafety.Api.Integrations.Documents.Controllers.V1;

/// <summary>V1 controller for Integrations.Documents read endpoints.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/integrations/documents")]
[Authorize(Policy = DocumentsAuthorizationPolicies.Read)]
[Tags("Documents")]
public sealed class DocumentsV1Controller : ControllerBase
{
    private readonly ISender _sender;

    /// <summary>Initialises a new instance of <see cref="DocumentsV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    public DocumentsV1Controller(ISender sender) => _sender = sender;

    /// <summary>Returns Integrations.Documents data for the resolved tenant.</summary>
    /// <param name="request">Query parameters bound from the query string.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the Integrations.Documents result set.</returns>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<DocumentsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAsync(
        [FromQuery] GetDocumentsRequest request,
        CancellationToken cancellationToken)
    {
        var correlationId = HttpContext.Items[PlantSafety.Api.Middleware.CorrelationIdMiddleware.ItemsKey]?.ToString()
                            ?? string.Empty;

        var query  = new GetDocumentsQuery(request.TenantId, request.SiteId, request.From, request.To ?? DateTime.UtcNow);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<DocumentsResponse>.Fail(result.Error.Message, correlationId));

        var response = new DocumentsResponse(
            result.Value.TenantId,
            result.Value.SiteId,
            result.Value.GeneratedAt,
            result.Value.Items);

        return Ok(ApiResponse<DocumentsResponse>.Ok(response, correlationId));
    }

    /// <summary>Returns a paginated, optionally category (document-type) and search filtered list of Integrations.Documents data.</summary>
    /// <param name="request">Query parameters bound from the query string.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the paginated Integrations.Documents result set.</returns>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("list")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<DocumentsItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetListAsync(
        [FromQuery] ListDocumentsRequest request,
        CancellationToken cancellationToken)
    {
        var correlationId = HttpContext.Items[PlantSafety.Api.Middleware.CorrelationIdMiddleware.ItemsKey]?.ToString()
                            ?? string.Empty;

        var query = new ListDocumentsQuery(
            request.TenantId, request.SiteId, request.Category, request.Search, request.PageNumber, request.PageSize);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<PaginatedResult<DocumentsItemDto>>.Fail(result.Error.Message, correlationId));

        return Ok(ApiResponse<PaginatedResult<DocumentsItemDto>>.Ok(result.Value, correlationId));
    }

    /// <summary>Returns the detail for a single Integrations.Documents record by id.</summary>
    /// <param name="documentId">The synced document record identifier.</param>
    /// <param name="tenantId">Tenant identifier resolved from JWT claims by TenantResolutionMiddleware.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the Integrations.Documents detail record.</returns>
    /// <response code="200">Document returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">No matching document was found for the caller's tenant.</response>
    [HttpGet("{documentId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<DocumentsItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<DocumentsItemDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(
        Guid documentId,
        [FromQuery] Guid tenantId,
        CancellationToken cancellationToken)
    {
        var correlationId = HttpContext.Items[PlantSafety.Api.Middleware.CorrelationIdMiddleware.ItemsKey]?.ToString()
                            ?? string.Empty;

        var query = new GetDocumentByIdQuery(tenantId, documentId);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<DocumentsItemDto>.Fail(result.Error.Message, correlationId));

        if (result.Value is null)
            return NotFound(ApiResponse<DocumentsItemDto>.Fail("Document not found.", correlationId));

        return Ok(ApiResponse<DocumentsItemDto>.Ok(result.Value, correlationId));
    }
}
