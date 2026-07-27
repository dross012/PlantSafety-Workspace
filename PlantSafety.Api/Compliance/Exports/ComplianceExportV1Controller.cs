// ============================================================================
// File: ComplianceExportV1Controller.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: V1 controller for Compliance module export. Covers all Compliance
//              sub-resources (AuditFindings, Inspections, CorrectiveActions, etc.)
//              via a unified export endpoint with an optional ResourceType filter.
// ============================================================================
//
// Onboarding Notes:
// - Route: GET api/v1/compliance/export
// - Auth policy: Compliance.Export
// - Compliance exports are the most compliance-sensitive in the platform.
//   Pdf format is recommended for regulatory submissions (locked, non-editable).
// - ResourceType query parameter (optional, e.g. "AuditFindings", "Inspections")
//   lets callers scope the export to a specific Compliance sub-resource. When
//   null, all Compliance records within the date range are exported.
// - Application layer reference:
//     PlantSafety.Application.Compliance.Queries.ExportComplianceQuery
//     PlantSafety.Application.Compliance.Queries.ExportComplianceResult
//
// ============================================================================

using PlantSafety.Api.Attributes;
using PlantSafety.Api.Filters;
using PlantSafety.Api.Models.Exports;
using Asp.Versioning;

namespace PlantSafety.Api.Compliance.Exports;

/// <summary>V1 controller for Compliance module bulk export.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/compliance/export")]
[RequiresTenant(AllowGlobalTenant = true)]
public sealed class ComplianceExportV1Controller : Controllers.BaseApiController
{
    private readonly ISender _sender;

    /// <summary>Initialises a new instance of <see cref="ComplianceExportV1Controller"/>.</summary>
    public ComplianceExportV1Controller(ISender sender) => _sender = sender;

    /// <summary>
    /// Generates a compliance data export and returns a presigned download URL.
    /// </summary>
    /// <param name="request">Export parameters including format, date range, status filter.</param>
    /// <param name="resourceType">
    /// Optional Compliance sub-resource type to scope the export
    /// (e.g. <c>"AuditFindings"</c>, <c>"CorrectiveActions"</c>, <c>"Inspections"</c>).
    /// When omitted all Compliance records are exported.
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    [HttpGet]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ServiceFilter(typeof(ExportValidationFilter))]
    public async Task<IActionResult> ExportAsync(
        [FromQuery] ExportRequest request,
        [FromQuery] string?        resourceType,
        CancellationToken          ct)
    {
        var query  = new PlantSafety.Application.Compliance.Queries.ExportComplianceQuery(
            request.TenantId,
            request.SiteId,
            (int)request.Format,
            request.From,
            request.To ?? DateTime.UtcNow,
            request.Status,
            resourceType,
            request.MaxRecords);

        var result = await _sender.Send(query, ct);

        if (result.IsFailure)
            return ApiBadRequest<ExportResponse>(result.Error.Message);

        return ApiOk(new ExportResponse(
            result.Value.FileName,
            result.Value.ContentType,
            result.Value.DownloadUrl,
            result.Value.ExpiresAt,
            result.Value.RecordCount));
    }
}
