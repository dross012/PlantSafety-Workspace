// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Query to retrieve Alert logging/reporting history within a UTC date range,
//   per "14. Alert Logging & Reporting".
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Alerts.Models;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Alerts.Queries;

/// <summary>
/// Retrieves Alert history (timestamp, location, severity, category, trigger source, response
/// actions, close-out verification) for the given tenant/site within the inclusive UTC date
/// range [<paramref name="From"/>, <paramref name="To"/>].
/// </summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site filter.</param>
/// <param name="From">Inclusive start of the date range (UTC).</param>
/// <param name="To">Inclusive end of the date range (UTC).</param>
/// <param name="PageNumber">1-based page number.</param>
/// <param name="PageSize">Page size, capped by <see cref="Common.Constants.PaginationConstants"/>.</param>
public sealed record GetAlertHistoryQuery(
    Guid TenantId,
    Guid? SiteId,
    DateTime From,
    DateTime To,
    int PageNumber,
    int PageSize) : IRequest<Result<PaginatedResult<AlertDto>>>;
