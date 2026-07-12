// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the Maintenance read-model.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Procedures.Maintenance.Models;

namespace PlantSafety.Application.Compliance.Procedures.Maintenance.Repositories;

/// <summary>Read-only repository for Maintenance data. Implemented in the Infrastructure layer.</summary>
public interface IMaintenanceRepository
{
    /// <summary>Returns Maintenance items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<MaintenanceItemDto>> GetMaintenanceProceduresAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
