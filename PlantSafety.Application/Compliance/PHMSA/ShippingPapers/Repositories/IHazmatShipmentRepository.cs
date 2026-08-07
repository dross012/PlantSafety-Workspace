// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.ShippingPapers
//  Layer:          Application
//  File:           IHazmatShipmentRepository.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for the HazmatShipment read-model.
//                  Implemented in the Infrastructure layer. Distinct from
//                  PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.Interfaces.
//                  IHazmatShipmentRepository (the write-side aggregate repository) -- same simple
//                  name, different namespace, mirroring
//                  PlantSafety.Application.Compliance.Permits.Repositories.IPermitsRepository's
//                  identical precedent for CQRS read/write repository separation.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.PHMSA.ShippingPapers.Models;

namespace PlantSafety.Application.Compliance.PHMSA.ShippingPapers.Repositories;

/// <summary>Read-only repository for HazmatShipment data. Implemented in the Infrastructure layer.</summary>
public interface IHazmatShipmentRepository
{
    /// <summary>
    /// Returns the HazmatShipment read-model record for the given identity, scoped to the given
    /// tenant, or <see langword="null"/> if not found or if the record belongs to a different tenant.
    /// </summary>
    Task<HazmatShipmentDto?> GetByIdAsync(Guid hazmatShipmentId, Guid tenantId, CancellationToken cancellationToken);

    /// <summary>Returns HazmatShipment items for the tenant/site, optionally filtered by Status and/or Mode.</summary>
    Task<IReadOnlyList<HazmatShipmentDto>> ListAsync(
        Guid tenantId,
        Guid? siteId,
        string? status,
        string? mode,
        CancellationToken cancellationToken);
}
