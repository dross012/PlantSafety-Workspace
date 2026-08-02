// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Compliance
//  Layer:          Application
//  File:           IComplianceRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for the (self-named) Compliance read-model.
//                  Implemented in the Infrastructure layer. Distinct from
//                  PlantSafety.Domain.Compliance.Compliance.Interfaces.IComplianceRepository (the
//                  write-side aggregate repository) -- same simple name, different namespace,
//                  mirroring
//                  PlantSafety.Application.Compliance.Permits.Repositories.IPermitsRepository's
//                  identical precedent for CQRS read/write repository separation.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Compliance.Models;

namespace PlantSafety.Application.Compliance.Compliance.Repositories;

/// <summary>Read-only repository for Compliance data. Implemented in the Infrastructure layer.</summary>
public interface IComplianceRepository
{
    /// <summary>
    /// Returns the Compliance read-model record for the given identity, scoped to the given
    /// tenant, or <see langword="null"/> if not found or if the record belongs to a different tenant.
    /// </summary>
    Task<ComplianceItemDto?> GetByIdAsync(Guid complianceId, Guid tenantId, CancellationToken cancellationToken);
}
