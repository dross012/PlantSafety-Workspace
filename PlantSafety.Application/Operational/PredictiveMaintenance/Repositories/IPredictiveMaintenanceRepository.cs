// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the PredictiveMaintenance read-model.
// SchemaVersion: 1

using PlantSafety.Application.Operational.PredictiveMaintenance.Models;

namespace PlantSafety.Application.Operational.PredictiveMaintenance.Repositories;

/// <summary>Read-only repository for PredictiveMaintenance data. Implemented in the Infrastructure layer.</summary>
public interface IPredictiveMaintenanceRepository
{
    /// <summary>Returns PredictiveMaintenance items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<PredictiveMaintenanceItemDto>> GetPredictiveMaintenanceAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
