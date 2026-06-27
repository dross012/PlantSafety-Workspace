// ============================================================================
// File: ContinuousImprovementExportV1Controller.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: V1 controller for ContinuousImprovement module export. Covers
//              kaizen events, improvement initiatives, and lessons-learned records.
// ============================================================================
//
// Onboarding Notes:
// - Route: GET api/v1/continuous-improvement/export
// - Auth policy: ContinuousImprovement.Export
// - ContinuousImprovement exports are used for management review meetings and
//   ISO 45001 continuous improvement documentation. Excel is the common format.
// - Application layer reference:
//     PlantSafety.Application.ContinuousImprovement.Queries.ExportContinuousImprovementQuery
//     PlantSafety.Application.ContinuousImprovement.Queries.ExportContinuousImprovementResult
//
// ============================================================================

using PlantSafety.Api.Attributes;
using PlantSafety.Api.Filters;
using PlantSafety.Api.Models.Exports;
using Asp.Versioning;

namespace PlantSafety.Api.ContinuousImprovement.Exports;

/// <summary>V1 controller for ContinuousImprovement module bulk export.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/continuous-improvement/export")]
[RequiresTenant(AllowGlobalTenant = true)]
public sealed class ContinuousImprovementExportV1Controller : Controllers.BaseApiController
{
    private readonly ISender _sender;

    /// <summary>Initialises a new instance of <see cref="ContinuousImprovementExportV1Controller"/>.</summary>
    public ContinuousImprovementExportV1Controller(ISender sender) => _sender = sender;

    /// <summary>
    /// Generates a continuous improvement records export and returns a presigned download URL.
    /// </summary>
    /// <param name="request">Export parameters.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpGet]
    [Authorize(Policy = "ContinuousImprovement.Export")]
    [ServiceFilter(typeof(ExportValidationFilter))]
    public async Task<IActionResult> ExportAsync(
        [FromQuery] ExportRequest request,
        CancellationToken         ct)
    {
        var query  = new PlantSafety.Application.ContinuousImprovement.Queries.ExportContinuousImprovementQuery(
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
