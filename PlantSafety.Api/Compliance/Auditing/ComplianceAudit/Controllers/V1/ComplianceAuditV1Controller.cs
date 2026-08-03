// ============================================================================
// File: ComplianceAuditV1Controller.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: V1 MVC controller exposing GET endpoints for the ComplianceAudit read-model.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow /api/v1/compliance/auditing/complianceaudit/...
//
// Rationale:
// - Routes all reads through MediatR ISender to the Application layer. No domain logic or data access here.
//
// Future Extension Seams:
// - Add [OutputCache(PolicyName = ...)] attributes when tenant-scoped cache policies are wired in ServiceCollectionExtensions.
//
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Compliance.Auditing.ComplianceAudit.Contracts.Requests;
using PlantSafety.Api.Compliance.Auditing.ComplianceAudit.Contracts.Responses;
using PlantSafety.Api.Compliance.Auditing.ComplianceAudit.Authorization;
using PlantSafety.Application.Compliance.Auditing.ComplianceAudit.Queries;

namespace PlantSafety.Api.Compliance.Auditing.ComplianceAudit.Controllers.V1;

/// <summary>V1 controller for ComplianceAudit read endpoints.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/compliance/auditing/complianceaudit")]
[Authorize(Policy = ComplianceAuditAuthorizationPolicies.Read)]
[Tags("ComplianceAudit")]
public sealed class ComplianceAuditV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="ComplianceAuditV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">
    /// Resolves the trusted, server-side TenantId from the authenticated JWT claims
    /// (Zero-Trust tenant isolation -- never trust a client-supplied TenantId).
    /// </param>
    public ComplianceAuditV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Returns ComplianceAudit data for the resolved tenant.</summary>
    /// <param name="request">Query parameters bound from the query string.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the ComplianceAudit result set.</returns>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<ComplianceAuditResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAsync(
        [FromQuery] GetComplianceAuditRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var correlationId = env.CorrelationId.ToString();

        var query  = new GetComplianceAuditQuery(env.TenantId, request.SiteId, request.From, request.To ?? DateTime.UtcNow);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<ComplianceAuditResponse>.Fail(result.Error.Message, correlationId));

        var response = new ComplianceAuditResponse(
            result.Value.TenantId,
            result.Value.SiteId,
            result.Value.GeneratedAt,
            result.Value.Items);

        return Ok(ApiResponse<ComplianceAuditResponse>.Ok(response, correlationId));
    }
}
