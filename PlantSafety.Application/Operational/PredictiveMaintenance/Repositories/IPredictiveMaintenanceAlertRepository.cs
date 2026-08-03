// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PredictiveMaintenance
//  Layer:          Application
//  File:           IPredictiveMaintenanceAlertRepository.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for a single PredictiveMaintenanceAlert
//                  record by identity. Implemented in the Infrastructure layer. Distinct from:
//                    - PlantSafety.Domain.Operational.PredictiveMaintenance.Interfaces
//                      .IPredictiveMaintenanceAlertRepository (the write-side aggregate repository
//                      -- same simple name, different namespace);
//                    - PlantSafety.Application.Operational.PredictiveMaintenance.Repositories
//                      .IPredictiveMaintenanceRepository (the pre-existing date-range list-read
//                      interface, untouched by this pass).
//                  Mirrors PlantSafety.Application.Compliance.Permits.Repositories.IPermitsRepository's
//                  identical CQRS read/write repository separation precedent.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Operational.PredictiveMaintenance.Models;

namespace PlantSafety.Application.Operational.PredictiveMaintenance.Repositories;

/// <summary>Read-only repository for a single PredictiveMaintenanceAlert record. Implemented in the Infrastructure layer.</summary>
public interface IPredictiveMaintenanceAlertRepository
{
    /// <summary>
    /// Returns the PredictiveMaintenanceAlert read-model record for the given identity, scoped to
    /// the given tenant, or <see langword="null"/> if not found or if it belongs to a different tenant.
    /// </summary>
    Task<PredictiveMaintenanceAlertItemDto?> GetByIdAsync(Guid predictiveMaintenanceAlertId, Guid tenantId, CancellationToken cancellationToken);

    /// <summary>
    /// Returns a paginated page of PredictiveMaintenanceAlert read-model records for the given
    /// tenant, optionally filtered by equipment and/or restricted to alerts that have not yet
    /// reached a terminal status. Backs the Predictive Maintenance Insights sub-resources
    /// (risk-summary, equipment-health, predictions list, equipment predictive view) added
    /// 2026-08-03. Additive-only evolution -- does not change <see cref="GetByIdAsync"/>.
    /// </summary>
    Task<(IReadOnlyList<PredictiveMaintenanceAlertItemDto> Items, int TotalCount)> GetPagedAsync(
        Guid tenantId,
        Guid? equipmentId,
        bool activeOnly,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);
}
