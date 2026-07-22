// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Application-layer repository abstraction for Dashboard aggregate persistence
//   and read-model queries. Implemented in the Infrastructure layer (Dapper/stored-procedure-only;
//   not included in this Domain+Application-only generation pass).
// SchemaVersion: 1

using PlantSafety.Application.Dashboards.Dashboard.Models;
using PlantSafety.Domain.Dashboards.Dashboard.ValueObjects.Ids;

namespace PlantSafety.Application.Dashboards.Dashboard.Repositories;

/// <summary>Application-layer persistence and read-model gateway for the Dashboard aggregate.</summary>
public interface IDashboardRepository
{
    /// <summary>Retrieves a Dashboard aggregate by identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.Dashboards.Dashboard.Aggregates.Dashboard?> GetByIdAsync(DashboardId dashboardId, CancellationToken cancellationToken);

    /// <summary>Persists a newly created Dashboard aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.Dashboards.Dashboard.Aggregates.Dashboard aggregate, CancellationToken cancellationToken);

    /// <summary>Persists changes to an existing Dashboard aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.Dashboards.Dashboard.Aggregates.Dashboard aggregate, CancellationToken cancellationToken);

    /// <summary>Retrieves a Dashboard read-model DTO by identifier, or <see langword="null"/> if not found.</summary>
    Task<DashboardDto?> GetDtoByIdAsync(Guid tenantId, Guid dashboardId, CancellationToken cancellationToken);

    /// <summary>Returns all published dashboards visible to the given viewer role.</summary>
    Task<IReadOnlyList<DashboardDto>> GetDashboardsForRoleAsync(Guid tenantId, string viewerRole, CancellationToken cancellationToken);
}
