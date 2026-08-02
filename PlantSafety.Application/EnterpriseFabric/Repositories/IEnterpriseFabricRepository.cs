// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnterpriseFabric
//  Layer:          Application
//  File:           IEnterpriseFabricRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for the EnterpriseFabric read-model.
//                  Implemented in the Infrastructure layer. Distinct from
//                  PlantSafety.Domain.EnterpriseFabric.Repositories.IEnterpriseFabricRepository
//                  (the write-side aggregate repository) -- same simple name, different
//                  namespace, mirroring
//                  PlantSafety.Application.Compliance.Auditing.Repositories.IAuditingRepository's
//                  identical precedent for CQRS read/write repository separation.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.EnterpriseFabric.Models;

namespace PlantSafety.Application.EnterpriseFabric.Repositories;

/// <summary>Read-only repository for EnterpriseFabric data. Implemented in the Infrastructure layer.</summary>
public interface IEnterpriseFabricRepository
{
    /// <summary>
    /// Returns the EnterpriseFabric read-model record for the given identity, scoped to the given
    /// tenant, or <see langword="null"/> if not found or if the record belongs to a different
    /// tenant.
    /// </summary>
    Task<EnterpriseFabricItemDto?> GetByIdAsync(Guid enterpriseFabricId, Guid tenantId, CancellationToken cancellationToken);
}
