// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Placarding
//  Layer:          Application
//  File:           IHazmatContainerRepository.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for the HazmatContainer read-model.
//                  Implemented in the Infrastructure layer. Distinct from
//                  PlantSafety.Domain.Compliance.PHMSA.Placarding.Interfaces.
//                  IHazmatContainerRepository (the write-side aggregate repository) -- same simple
//                  name, different namespace, mirroring
//                  PlantSafety.Application.Compliance.Permits.Repositories.IPermitsRepository's
//                  identical precedent for CQRS read/write repository separation.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.PHMSA.Placarding.Models;

namespace PlantSafety.Application.Compliance.PHMSA.Placarding.Repositories;

/// <summary>Read-only repository for HazmatContainer data. Implemented in the Infrastructure layer.</summary>
public interface IHazmatContainerRepository
{
    /// <summary>
    /// Returns the HazmatContainer read-model record for the given identity, scoped to the given
    /// tenant, or <see langword="null"/> if not found or if the record belongs to a different tenant.
    /// </summary>
    Task<HazmatContainerDto?> GetByIdAsync(Guid hazmatContainerId, Guid tenantId, CancellationToken cancellationToken);

    /// <summary>Returns HazmatContainer items for the tenant/site, optionally filtered by MaterialId.</summary>
    Task<IReadOnlyList<HazmatContainerDto>> ListAsync(
        Guid tenantId,
        Guid? siteId,
        Guid? materialId,
        CancellationToken cancellationToken);
}
