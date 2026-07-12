// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the PreventiveMaintenance read-model.
// SchemaVersion: 1

using PlantSafety.Application.Operational.PreventiveMaintenance.Models;

namespace PlantSafety.Application.Operational.PreventiveMaintenance.Repositories;

/// <summary>Read-only repository for PreventiveMaintenance data. Implemented in the Infrastructure layer.</summary>
public interface IPreventiveMaintenanceRepository
{
    /// <summary>Returns PreventiveMaintenance items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<PreventiveMaintenanceItemDto>> GetPreventiveMaintenanceAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
