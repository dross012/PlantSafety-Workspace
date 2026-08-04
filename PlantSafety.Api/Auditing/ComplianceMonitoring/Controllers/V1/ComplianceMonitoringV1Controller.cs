// ============================================================================
// File: ComplianceMonitoringV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (Phase 7 §6.3 Support Tooling)
// Created: 2026-08-03
// Description: V1 controller exposing the Auditing.ComplianceMonitoring bounded context's
//              tenant-scoped audit stream (GetTenantAuditStreamQuery) to the Admin Portal's
//              Support Console log/event viewer. This endpoint was genuinely missing prior to
//              Phase 7 §6.3 (the Application layer's query/handler existed with no
//              corresponding Api controller); added here as a thin HTTP boundary rather than
//              inventing a fictional route in the UI layer.
// ============================================================================
//
// Onboarding Notes:
// - Routes:
//   GET api/v1/auditing/compliance-monitoring/tenants/{tenantId}/audit-stream?fromUtc=&toUtc=
//   GET api/v1/auditing/compliance-monitoring/tenants/{tenantId}/compliance-report?fromUtc=&toUtc=
//     (added 2026-08-03, Phase 9 §5.6 Compliance Report Generation -- see
//     GetComplianceReportQuery's own file header for the governance-validation finding this
//     closes: TRIR/LTIR, incident/audit lineage, and cross-site actions existed as three
//     independent real, queryable sources with no unified compliance-report endpoint.)
//
// ============================================================================

using PlantSafety.Api.Auditing.ComplianceMonitoring.Authorization;
using PlantSafety.Api.Common;
using PlantSafety.Api.Context;
using PlantSafety.Application.Auditing.ComplianceMonitoring.ComplianceReport.Queries;
using PlantSafety.Application.Auditing.ComplianceMonitoring.PermissionUsage.Queries;

namespace PlantSafety.Api.Auditing.ComplianceMonitoring.Controllers.V1;

/// <summary>V1 controller for the Auditing.ComplianceMonitoring bounded context's tenant audit stream.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auditing/compliance-monitoring")]
[Authorize(Policy = ComplianceMonitoringAuthorizationPolicies.ViewLogs)]
[Tags("Auditing.ComplianceMonitoring")]
public sealed class ComplianceMonitoringV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="ComplianceMonitoringV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public ComplianceMonitoringV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Retrieves the tenant-scoped authorization-decision (permission usage) audit stream for a UTC range.</summary>
    /// <param name="tenantId">Tenant identifier to scope the audit stream to.</param>
    /// <param name="fromUtc">Inclusive UTC range start. Defaults to 24 hours before <paramref name="toUtc"/>.</param>
    /// <param name="toUtc">Inclusive UTC range end. Defaults to now.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("tenants/{tenantId:guid}/audit-stream")]
    [ProducesResponseType(typeof(ApiResponse<TenantAuditStreamResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TenantAuditStreamResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTenantAuditStreamAsync(
        Guid tenantId,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var effectiveTo = toUtc ?? DateTime.UtcNow;
        var effectiveFrom = fromUtc ?? effectiveTo.AddHours(-24);

        var query = new GetTenantAuditStreamQuery(tenantId, effectiveFrom, effectiveTo);
        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>
    /// Retrieves a unified compliance report for a tenant/UTC range -- TRIR/LTIR safety KPIs,
    /// audit-trail/incident lineage, and cross-site coordinated-action history, composed into a
    /// single queryable/exportable response.
    /// </summary>
    /// <param name="tenantId">Tenant identifier to scope the report to.</param>
    /// <param name="fromUtc">Inclusive UTC range start. Defaults to 30 days before <paramref name="toUtc"/>.</param>
    /// <param name="toUtc">Inclusive UTC range end. Defaults to now.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("tenants/{tenantId:guid}/compliance-report")]
    [ProducesResponseType(typeof(ApiResponse<Application.Auditing.ComplianceMonitoring.ComplianceReport.Models.ComplianceReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<Application.Auditing.ComplianceMonitoring.ComplianceReport.Models.ComplianceReportDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetComplianceReportAsync(
        Guid tenantId,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var effectiveTo = toUtc ?? DateTime.UtcNow;
        var effectiveFrom = fromUtc ?? effectiveTo.AddDays(-30);

        var query = new GetComplianceReportQuery(tenantId, effectiveFrom, effectiveTo);
        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
