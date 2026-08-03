// ============================================================================
// File: GetReportingRequest.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: External-facing DTO for GET /analytics/reporting query parameters, bound from query string by the V1 controller.
// ============================================================================
//
// Onboarding Notes:
// - Validated by FluentValidation via ApiValidationFilter before MediatR dispatch.
//
// Rationale:
// - Decouples the external HTTP contract from internal Application-layer query types.
//
// Future Extension Seams:
// - Add cursor/page fields here when Reporting endpoints need server-side paging.
//
// ============================================================================

namespace PlantSafety.Api.Analytics.Reporting.Contracts.Requests;

/// <summary>Query parameters for retrieving Reporting data.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims by TenantResolutionMiddleware.</param>
/// <param name="SiteId">Optional site filter. Null returns data for all accessible sites.</param>
/// <param name="From">Inclusive start of the date range (UTC).</param>
/// <param name="To">Inclusive end of the date range (UTC). Defaults to UtcNow when not supplied.</param>
public sealed record GetReportingRequest(
    Guid      TenantId,
    Guid?     SiteId,
    DateTime  From,
    DateTime? To);

/// <summary>
/// Query parameters for retrieving a paginated, optionally type-filtered page of the report
/// catalog. Backs the PlantSafety.UI Reporting module's Reports List page
/// (GET /analytics/reporting/reports). TenantId is intentionally NOT bound here -- it is
/// resolved server-side from the caller's JWT via <see cref="PlantSafety.Api.Context.RequestEnvelopeFactory"/>
/// so a caller cannot spoof another tenant's data via the query string.
/// </summary>
/// <param name="SiteId">Optional site filter. Null returns data for all accessible sites.</param>
/// <param name="ReportType">Optional report type/category filter (e.g. "Safety"). Null returns all types.</param>
/// <param name="PageNumber">1-based page number.</param>
/// <param name="PageSize">Items per page.</param>
public sealed record ListReportsRequest(
    Guid?   SiteId,
    string? ReportType,
    int     PageNumber = 1,
    int     PageSize = 25);