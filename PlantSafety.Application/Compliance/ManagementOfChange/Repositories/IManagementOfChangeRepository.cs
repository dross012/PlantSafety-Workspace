// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.ManagementOfChange
//  Layer:          Application
//  File:           IManagementOfChangeRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for the ManagementOfChange read-model.
//                  Implemented in the Infrastructure layer. Distinct from
//                  PlantSafety.Domain.Compliance.ManagementOfChange.Interfaces.IManagementOfChangeRepository
//                  (the write-side aggregate repository) -- same simple name, different namespace,
//                  mirroring PlantSafety.Application.Compliance.Permits.Repositories.IPermitsRepository's
//                  identical precedent for CQRS read/write repository separation.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.ManagementOfChange.Models;

namespace PlantSafety.Application.Compliance.ManagementOfChange.Repositories;

/// <summary>Read-only repository for ManagementOfChange data. Implemented in the Infrastructure layer.</summary>
public interface IManagementOfChangeRepository
{
    /// <summary>
    /// Returns the ManagementOfChange read-model record for the given identity, scoped to the given
    /// tenant, or <see langword="null"/> if not found or if the record belongs to a different tenant.
    /// </summary>
    Task<ManagementOfChangeItemDto?> GetByIdAsync(Guid managementOfChangeId, Guid tenantId, CancellationToken cancellationToken);
}
