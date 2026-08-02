// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability
//  Layer:          Application
//  File:           ISustainabilityRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for the Sustainability read-model.
//                  Implemented in the Infrastructure layer. Distinct from
//                  PlantSafety.Domain.Sustainability.Repositories.ISustainabilityRepository (the
//                  write-side aggregate repository) -- same simple name, different namespace,
//                  mirroring
//                  PlantSafety.Application.Compliance.Auditing.Repositories.IAuditingRepository's
//                  identical precedent for CQRS read/write repository separation. Also distinct
//                  from the pre-existing, unrelated Features-vertical
//                  PlantSafety.Application.Features.Sustainability.Handlers.ISustainabilityRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Sustainability.Models;

namespace PlantSafety.Application.Sustainability.Repositories;

/// <summary>Read-only repository for Sustainability data. Implemented in the Infrastructure layer.</summary>
public interface ISustainabilityRepository
{
    /// <summary>
    /// Returns the Sustainability read-model record for the given identity, scoped to the given
    /// tenant, or <see langword="null"/> if not found or if the record belongs to a different
    /// tenant.
    /// </summary>
    Task<SustainabilityItemDto?> GetByIdAsync(Guid sustainabilityId, Guid tenantId, CancellationToken cancellationToken);
}
