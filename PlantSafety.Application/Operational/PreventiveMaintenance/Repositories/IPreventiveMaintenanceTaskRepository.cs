// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PreventiveMaintenance
//  Layer:          Application
//  File:           IPreventiveMaintenanceTaskRepository.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for the PreventiveMaintenanceTask by-id
//                  read-model. Implemented in the Infrastructure layer. Distinct simple name from
//                  both the pre-existing Application-layer IPreventiveMaintenanceRepository
//                  (date-range list read) AND the Domain-layer (write-side)
//                  IPreventiveMaintenanceTaskRepository -- same simple name as the Domain
//                  interface, different namespace, disambiguated via fully-qualified DI
//                  registration, mirroring
//                  PlantSafety.Application.Compliance.Permits.Repositories.IPermitsRepository's
//                  identical precedent for CQRS read/write repository separation.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Operational.PreventiveMaintenance.Models;

namespace PlantSafety.Application.Operational.PreventiveMaintenance.Repositories;

/// <summary>Read-only repository for PreventiveMaintenanceTask by-id data. Implemented in the Infrastructure layer.</summary>
public interface IPreventiveMaintenanceTaskRepository
{
    /// <summary>
    /// Returns the PreventiveMaintenanceTask read-model record for the given identity, scoped to
    /// the given tenant, or <see langword="null"/> if not found or if the record belongs to a
    /// different tenant.
    /// </summary>
    Task<PreventiveMaintenanceTaskItemDto?> GetByIdAsync(Guid preventiveMaintenanceTaskId, Guid tenantId, CancellationToken cancellationToken);
}
