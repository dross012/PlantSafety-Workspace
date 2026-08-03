// ============================================================================
// File: AnalyticsExportV1Controller.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: V1 controller for Analytics module export. Dispatches to the
//              Application layer ExportAnalyticsQuery and returns a presigned
//              Azure Blob download URL wrapped in ApiResponse<ExportResponse>.
// ============================================================================
//
// Onboarding Notes:
// - Route: GET api/v1/analytics/export
// - Auth policy: Analytics.Export (defined in AuthorizationPolicyExtensions)
// - ExportValidationFilter runs before the action to validate ExportRequest params.
// - The Application layer query references:
//     PlantSafety.Application.Analytics.Queries.ExportAnalyticsQuery
//     PlantSafety.Application.Analytics.Queries.ExportAnalyticsResult
//   These must be defined in PlantSafety.Application before this controller compiles.
// - ExportAnalyticsQuery accepts format, date range, and MaxRecords and returns a
//   presigned SAS URL to the generated file in Azure Blob Storage.
// - AllowGlobalTenant = true because analytics exports are typically tenant-wide.
//
// ============================================================================

using PlantSafety.Api.Attributes;
using PlantSafety.Api.Filters;
using PlantSafety.Api.Models.Exports;
using Asp.Versioning;

namespace PlantSafety.Api.Analytics.Exports;

/// <summary>V1 controller for Analytics module bulk export.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/analytics/export")]
[RequiresTenant(AllowGlobalTenant = true)]
public sealed class AnalyticsExportV1Controller : Controllers.BaseApiController
{
    private readonly ISender _sender;

    /// <summary>Initialises a new instance of <see cref="AnalyticsExportV1Controller"/>.</summary>
    public AnalyticsExportV1Controller(ISender sender) => _sender = sender;

    /// <summary>
    /// Generates an export file for analytics data and returns a presigned download URL.
    /// </summary>
    /// <param name="request">Export parameters including format, date range, and record cap.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 OK with <see cref="ExportResponse"/> containing the download URL,
    /// or 400 Bad Request if the Application layer returns a failure result.
    /// </returns>
    [HttpGet]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ServiceFilter(typeof(ExportValidationFilter))]
    public async Task<IActionResult> ExportAsync(
        [FromQuery] ExportRequest request,
        CancellationToken         ct)
    {
        var query  = new PlantSafety.Application.Analytics.Queries.ExportAnalyticsQuery(
            TenantId,
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
