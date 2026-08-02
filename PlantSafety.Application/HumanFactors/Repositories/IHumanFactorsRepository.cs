// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors
//  Layer:          Application
//  File:           IHumanFactorsRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for the HumanFactors read-model.
//                  Implemented in the Infrastructure layer. Distinct from
//                  PlantSafety.Domain.HumanFactors.Repositories.IHumanFactorsRepository (the
//                  write-side aggregate repository) -- same simple name, different namespace,
//                  mirroring PlantSafety.Application.Compliance.Auditing.Repositories.IAuditingRepository's
//                  identical precedent for CQRS read/write repository separation.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.HumanFactors.Models;

namespace PlantSafety.Application.HumanFactors.Repositories;

/// <summary>Read-only repository for HumanFactors data. Implemented in the Infrastructure layer.</summary>
public interface IHumanFactorsRepository
{
    /// <summary>
    /// Returns the HumanFactors read-model record for the given identity, scoped to the given
    /// tenant, or <see langword="null"/> if not found or if the record belongs to a different
    /// tenant.
    /// </summary>
    Task<HumanFactorsItemDto?> GetByIdAsync(Guid humanFactorsId, Guid tenantId, CancellationToken cancellationToken);
}
