// ============================================================================
// File: AuditReportV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (API Contract Validation Agent)
// Created: 2026-08-02
// Last Updated: 2026-08-02
// Description: V1 controller exposing the Auditing.AuditReport sub-feature (Phase 5 §6.2):
//              GenerateAuditReportCommand, GetAuditReportQuery, GetAuditReportHistoryQuery, and
//              GetAuditReportContentQuery (byte-content download). The content endpoint returns
//              the rendered bytes inside the standard ApiResponse&lt;T&gt; JSON envelope
//              (System.Text.Json serialises byte[] as base64), matching the existing
//              Compliance.Auditing OperationsAuditViewer CSV-export convention's payload shape and
//              exactly the wire contract PlantSafety.UI's plantSafetyDownloadFile JS-interop flow
//              (AuditReportsPage.razor) expects: FileName/ContentType/base64 Content.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes match
//   PlantSafety.UI.Modules.Auditing.ApiClients.AuditingApiClient's AuditReport methods exactly:
//   POST   api/v1/auditing/audit-reports?TenantId=
//   GET    api/v1/auditing/audit-reports/{auditReportId}?TenantId=
//   GET    api/v1/auditing/audit-reports/history?TenantId=&TargetEntityType=&TargetEntityId=
//   GET    api/v1/auditing/audit-reports/{auditReportId}/content?TenantId=
//
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Common;
using PlantSafety.Api.Auditing.AuditReport.Authorization;
using PlantSafety.Api.Auditing.AuditReport.Contracts.Requests;
using PlantSafety.Application.Auditing.AuditReport.Commands;
using PlantSafety.Application.Auditing.AuditReport.Models;
using PlantSafety.Application.Auditing.AuditReport.Queries;

namespace PlantSafety.Api.Auditing.AuditReport.Controllers.V1;

/// <summary>V1 controller for the Auditing.AuditReport sub-feature.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auditing/audit-reports")]
[Authorize(Policy = AuditReportAuthorizationPolicies.Read)]
[Tags("Auditing.AuditReport")]
public sealed class AuditReportV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="AuditReportV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public AuditReportV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Generates a printable/exportable AuditReport for a target entity.</summary>
    /// <param name="request">Caller-supplied report generation fields.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <remarks>
    /// TenantId is intentionally NOT bound from the client (query string or body). It is resolved
    /// server-side from the authenticated JWT claims via <see cref="RequestEnvelopeFactory"/>/
    /// <c>IOperationContext</c>, matching TenantResolutionMiddleware's Zero-Trust guarantee
    /// (Infrastructure/Security manual, Zero-Trust/mTLS section) that TenantId must never be
    /// inferred from client-supplied input. A client-supplied <c>TenantId</c> query parameter, if
    /// present on the wire, is ignored.
    /// </remarks>
    // Gated by Export (Approver-and-above), not Write. AuditReports are safety-critical,
    // compliance-critical, legally-discoverable chain-of-custody artifacts (Handbook Auditing &
    // Compliance rules) -- generation is a compliance act, not an ordinary domain write, so it must
    // sit at the same Approver tier as content download. This restores read/write tier symmetry
    // (Handbook §2.11.D) by raising generation to match download, rather than lowering download to
    // match generation -- see GetContentAsync's remarks for the tier-inversion this corrects.
    [HttpPost]
    [Authorize(Policy = AuditReportAuthorizationPolicies.Export)]
    [ProducesResponseType(typeof(ApiResponse<AuditReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuditReportDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GenerateAsync(
        [FromBody] GenerateAuditReportRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = GenerateAuditReportCommand.FromDto(
            tenantId: env.TenantId,
            siteId: request.SiteId,
            requestedBy: env.ActorId,
            targetEntityType: request.TargetEntityType,
            targetEntityId: request.TargetEntityId,
            format: request.Format,
            entityIdentityJson: request.EntityIdentityJson,
            approvalsJson: request.ApprovalsJson,
            exceptionsJson: request.ExceptionsJson,
            signaturesJson: request.SignaturesJson,
            correlationId: env.CorrelationId,
            causationId: env.CausationId,
            apiVersionToken: env.ApiVersionToken);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves a single AuditReport by identity.</summary>
    /// <param name="auditReportId">Identity of the report to retrieve.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("{auditReportId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<AuditReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByIdAsync(
        Guid auditReportId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetAuditReportQuery(env.TenantId, auditReportId);
        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves the AuditReport generation history for a target entity, most recent first.</summary>
    /// <param name="targetEntityType">Type discriminator of the entity.</param>
    /// <param name="targetEntityId">Identity of the entity.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("history")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AuditReportDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AuditReportDto>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetHistoryAsync(
        [FromQuery] string targetEntityType,
        [FromQuery] Guid targetEntityId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        if (string.IsNullOrWhiteSpace(targetEntityType))
            return BadRequest(ApiResponse<IReadOnlyList<AuditReportDto>>.Fail("TargetEntityType is required.", env.CorrelationId.ToString()));

        var query = new GetAuditReportHistoryQuery(env.TenantId, targetEntityType, targetEntityId);
        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>
    /// Retrieves the rendered byte-stream content of a generated AuditReport, for download.
    /// Returned inside the standard <see cref="ApiResponse{T}"/> envelope; <c>Content</c> is
    /// base64-encoded on the wire by System.Text.Json's default <c>byte[]</c> handling, matching
    /// the <c>plantSafetyDownloadFile</c> JS-interop flow's expected shape.
    /// </summary>
    /// <param name="auditReportId">Identity of the report whose content should be downloaded.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    // Handbook §2.11.D / read-write tier-separation rule: a GET action must never require a
    // stricter Safety tier than the write action that produced the artifact it reads. A prior pass
    // "fixed" the tier mismatch this endpoint had with GenerateAsync by downgrading this endpoint
    // from Export (Approver) to Write (Editor) -- that inverted the correct remedy: AuditReports are
    // safety-critical, compliance-critical, chain-of-custody artifacts, so the symmetry must be
    // restored at the stricter Approver tier, not the looser Editor tier. GenerateAsync was
    // therefore raised to Export instead, and this endpoint restored to Export, so generation and
    // download both require Approver-and-above.
    [HttpGet("{auditReportId:guid}/content")]
    [Authorize(Policy = AuditReportAuthorizationPolicies.Export)]
    [ProducesResponseType(typeof(ApiResponse<AuditReportContentResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetContentAsync(
        Guid auditReportId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetAuditReportContentQuery(env.TenantId, auditReportId);
        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
