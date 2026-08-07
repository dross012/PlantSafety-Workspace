// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Classification
//  Layer:          Application
//  File:           IHazmatMaterialRepository.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for the HazmatMaterial read-model.
//                  Implemented in the Infrastructure layer. Distinct from
//                  PlantSafety.Domain.Compliance.PHMSA.Classification.Interfaces.
//                  IHazmatMaterialRepository (the write-side aggregate repository) -- same simple
//                  name, different namespace, mirroring
//                  PlantSafety.Application.Compliance.Permits.Repositories.IPermitsRepository's
//                  identical precedent for CQRS read/write repository separation.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.PHMSA.Classification.Models;

namespace PlantSafety.Application.Compliance.PHMSA.Classification.Repositories;

/// <summary>Read-only repository for HazmatMaterial data. Implemented in the Infrastructure layer.</summary>
public interface IHazmatMaterialRepository
{
    /// <summary>
    /// Returns the HazmatMaterial read-model record for the given identity, scoped to the given
    /// tenant, or <see langword="null"/> if not found or if the record belongs to a different tenant.
    /// </summary>
    Task<HazmatMaterialDto?> GetByIdAsync(Guid hazmatMaterialId, Guid tenantId, CancellationToken cancellationToken);

    /// <summary>Returns HazmatMaterial items for the tenant/site, optionally filtered by UN number.</summary>
    Task<IReadOnlyList<HazmatMaterialDto>> ListAsync(
        Guid tenantId,
        Guid? siteId,
        string? unNumber,
        CancellationToken cancellationToken);
}
