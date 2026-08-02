// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Certification
//  Layer:          Application
//  File:           ICertificationRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for the Certification read-model.
//                  Implemented in the Infrastructure layer. Distinct from
//                  PlantSafety.Domain.Compliance.Certification.Interfaces.ICertificationRepository
//                  (the write-side aggregate repository) -- same simple name, different
//                  namespace, mirroring
//                  PlantSafety.Application.Compliance.Permits.Repositories.IPermitsRepository's
//                  identical precedent for CQRS read/write repository separation.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Certification.Models;

namespace PlantSafety.Application.Compliance.Certification.Repositories;

/// <summary>Read-only repository for Certification data. Implemented in the Infrastructure layer.</summary>
public interface ICertificationRepository
{
    /// <summary>
    /// Returns the Certification read-model record for the given identity, scoped to the given
    /// tenant, or <see langword="null"/> if not found or if the record belongs to a different tenant.
    /// </summary>
    Task<CertificationItemDto?> GetByIdAsync(Guid certificationId, Guid tenantId, CancellationToken cancellationToken);
}
