// ============================================================================
// File: OperationalExportV1Controller.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: V1 controller for Operational module export. Covers permits,
//              LOTO procedures, shift handovers, and task records via a unified
//              export endpoint.
// ============================================================================
//
// Onboarding Notes:
// - Route: GET api/v1/operational/export
// - Auth policy: Operational.Export
// - Operational exports are used for permit-to-work audit trails and LOTO
//   compliance records. Pdf format is strongly preferred for these use cases
//   as the records may need to be submitted to regulatory bodies.
// - Application layer reference:
//     PlantSafety.Application.Operational.Queries.ExportOperationalQuery
//     PlantSafety.Application.Operational.Queries.ExportOperationalResult
//
// ============================================================================

using PlantSafety.Api.Attributes;
using PlantSafety.Api.Filters;
using PlantSafety.Api.Models.Exports;
using Asp.Versioning;

namespace PlantSafety.Api.Operational.Exports;

/// <summary>V1 controller for Operational module bulk export.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/operational/export")]
[RequiresTenant(AllowGlobalTenant = true)]
public sealed class OperationalExportV1Controller : Controllers.BaseApiController
{
    private readonly ISender _sender;

    /// <summary>Initialises a new instance of <see cref="OperationalExportV1Controller"/>.</summary>
    public OperationalExportV1Controller(ISender sender) => _sender = sender;

    /// <summary>
    /// Generates an operational records export and returns a presigned download URL.
    /// </summary>
    /// <param name="request">Export parameters.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpGet]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ServiceFilter(typeof(ExportValidationFilter))]
    public async Task<IActionResult> ExportAsync(
        [FromQuery] ExportRequest request,
        CancellationToken         ct)
    {
        var query  = new PlantSafety.Application.Operational.Queries.ExportOperationalQuery(
            request.TenantId,
            request.SiteId,
            (int)request.Format,
            request.From,
            request.To ?? DateTime.UtcNow,
            request.Status,
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
