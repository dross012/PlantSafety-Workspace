// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Query to retrieve a paginated, optionally type-filtered page of the report
//   catalog for a tenant/site. Backs the PlantSafety.UI Reporting module's Reports List page.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Analytics.Reporting.Models;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Analytics.Reporting.Queries;

/// <summary>
/// Retrieves a deterministically ordered, paginated page of the report catalog for the given
/// tenant, optionally scoped to a single site and/or filtered by report type.
/// </summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site filter. Null returns data for all accessible sites.</param>
/// <param name="TypeFilter">Optional report type/category filter (e.g. "Safety"). Null returns all types.</param>
/// <param name="PageNumber">1-based page number.</param>
/// <param name="PageSize">Page size, capped by <see cref="Common.Constants.PaginationConstants"/>.</param>
public sealed record GetReportsPagedQuery(
    Guid TenantId,
    Guid? SiteId,
    string? TypeFilter,
    int PageNumber,
    int PageSize) : IRequest<Result<PaginatedResult<ReportListItemDto>>>;
