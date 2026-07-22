// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Query to retrieve a single Dashboard (with its resolved widget layout), per
//   "Dashboard API Queries: Widgets call GET /dashboard/{id}/widgets".
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Dashboards.Dashboard.Models;

namespace PlantSafety.Application.Dashboards.Dashboard.Queries;

/// <summary>Retrieves a single Dashboard by identifier, including its resolved widget layout.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="DashboardId">Identifier of the dashboard to retrieve.</param>
public sealed record GetDashboardByIdQuery(Guid TenantId, Guid DashboardId) : IRequest<Result<DashboardDto>>;
