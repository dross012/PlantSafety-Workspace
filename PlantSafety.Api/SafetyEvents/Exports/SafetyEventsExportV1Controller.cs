// ============================================================================
// File: SafetyEventsExportV1Controller.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: V1 controller for SafetyEvents module export. Covers incidents,
//              near-misses, hazard reports, and observations via a unified export
//              endpoint.
// ============================================================================
//
// Onboarding Notes:
// - Route: GET api/v1/safety-events/export
// - Auth policy: SafetyEvents.Export
// - SafetyEvents exports are frequently requested for regulatory reporting
//   (OSHA, HSE). Pdf and Excel formats are most commonly used.
// - Severity query parameter (optional: "Low", "Medium", "High", "Critical")
//   scopes the export to events at or above the specified severity level.
//   Maps to a filter in ExportSafetyEventsQuery.
// - Application layer reference:
//     PlantSafety.Application.SafetyEvents.Queries.ExportSafetyEventsQuery
//     PlantSafety.Application.SafetyEvents.Queries.ExportSafetyEventsResult
//
// ============================================================================

using PlantSafety.Api.Attributes;
using PlantSafety.Api.Filters;
using PlantSafety.Api.Models.Exports;
using Asp.Versioning;

namespace PlantSafety.Api.SafetyEvents.Exports;

/// <summary>V1 controller for SafetyEvents module bulk export.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/safety-events/export")]
[RequiresTenant(AllowGlobalTenant = true)]
public sealed class SafetyEventsExportV1Controller : Controllers.BaseApiController
{
    private readonly ISender _sender;

    /// <summary>Initialises a new instance of <see cref="SafetyEventsExportV1Controller"/>.</summary>
    public SafetyEventsExportV1Controller(ISender sender) => _sender = sender;

    /// <summary>
    /// Generates a safety events export and returns a presigned download URL.
    /// </summary>
    /// <param name="request">Export parameters.</param>
    /// <param name="severity">
    /// Optional minimum severity filter (<c>"Low"</c>, <c>"Medium"</c>, <c>"High"</c>, <c>"Critical"</c>).
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    [HttpGet]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ServiceFilter(typeof(ExportValidationFilter))]
    public async Task<IActionResult> ExportAsync(
        [FromQuery] ExportRequest request,
        [FromQuery] string?        severity,
        CancellationToken          ct)
    {
        var query  = new PlantSafety.Application.SafetyEvents.Queries.ExportSafetyEventsQuery(
            request.TenantId,
            request.SiteId,
            (int)request.Format,
            request.From,
            request.To ?? DateTime.UtcNow,
            request.Status,
            severity,
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
