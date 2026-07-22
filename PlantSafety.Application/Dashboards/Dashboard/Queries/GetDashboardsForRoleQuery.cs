// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Query to retrieve all published Dashboards visible to a given viewer role,
//   per "4. Role-Based Dashboards".
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Dashboards.Dashboard.Models;

namespace PlantSafety.Application.Dashboards.Dashboard.Queries;

/// <summary>Retrieves all published Dashboards visible to the given viewer role.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="ViewerRole">Viewer role code (e.g. "Executive", "SafetyManager", "Supervisor", "MaintenanceEngineering", "ContractorCoordinator").</param>
public sealed record GetDashboardsForRoleQuery(Guid TenantId, string ViewerRole) : IRequest<Result<IReadOnlyList<DashboardDto>>>;
