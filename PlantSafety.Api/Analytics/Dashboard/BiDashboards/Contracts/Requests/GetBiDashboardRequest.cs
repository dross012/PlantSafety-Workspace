// ============================================================================
// File: GetBiDashboardRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (API Contract Validation Agent)
// Created: 2026-08-02
// Last Updated: 2026-08-02
// Description: External-facing DTO shared by every GET api/v1/analytics/dashboard/{subfeature}
//              BI Dashboards endpoint, bound from query string by BiDashboardsV1Controller.
// ============================================================================
//
// Onboarding Notes:
// - Mirrors PlantSafety.Api.Analytics.Dashboard.Executive.Contracts.Requests.GetExecutiveRequest,
//   the established shape for every Analytics.Dashboard sub-feature read endpoint.
//
// Rationale:
// - Decouples the external HTTP contract from the internal Application-layer query types.
//
// Future Extension Seams:
// - Add cursor/page fields here when BiDashboards endpoints need server-side paging.
//
// ============================================================================

namespace PlantSafety.Api.Analytics.Dashboard.BiDashboards.Contracts.Requests;

/// <summary>Query parameters shared by every BI Dashboards KPI section endpoint.</summary>
/// <remarks>
/// TenantId is intentionally NOT a member of this DTO. Despite the misleading claim in a prior
/// revision of this doc comment, ASP.NET model binding would have bound a <c>TenantId</c> property
/// here directly from client-supplied query string input -- it is NOT resolved from JWT claims by
/// virtue of being named "TenantId". The trusted, server-resolved tenant is obtained by the
/// controller from <see cref="PlantSafety.Api.Context.RequestEnvelopeFactory"/> (backed by
/// <c>IOperationContext</c>, itself populated by TenantResolutionMiddleware from the authenticated
/// JWT's <c>tenantId</c> claim). See Infrastructure/Security manual, Zero-Trust tenant-isolation
/// requirement.
/// </remarks>
/// <param name="SiteId">Optional site filter. Null returns data for all accessible sites.</param>
/// <param name="From">Inclusive start of the date range (UTC).</param>
/// <param name="To">Inclusive end of the date range (UTC). Defaults to UtcNow when not supplied.</param>
public sealed record GetBiDashboardRequest(
    Guid?     SiteId,
    DateTime  From,
    DateTime? To);
