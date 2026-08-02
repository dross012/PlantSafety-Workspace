// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth
//  Layer:          Application
//  File:           IEnvironmentalHealthRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for the EnvironmentalHealth read-model.
//                  Implemented in the Infrastructure layer. Distinct from
//                  PlantSafety.Domain.EnvironmentalHealth.Repositories.IEnvironmentalHealthRepository
//                  (the write-side aggregate repository) -- same simple name, different
//                  namespace, mirroring
//                  PlantSafety.Application.Compliance.Auditing.Repositories.IAuditingRepository's
//                  identical precedent for CQRS read/write repository separation. Also distinct
//                  from the pre-existing, unrelated, LIVE
//                  Features.EnvironmentalHealth.Handlers.IEnvironmentalHealthReadingRepository
//                  "reading" vertical. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.EnvironmentalHealth.Models;

namespace PlantSafety.Application.EnvironmentalHealth.Repositories;

/// <summary>Read-only repository for EnvironmentalHealth data. Implemented in the Infrastructure layer.</summary>
public interface IEnvironmentalHealthRepository
{
    /// <summary>
    /// Returns the EnvironmentalHealth read-model record for the given identity, scoped to the
    /// given tenant, or <see langword="null"/> if not found or if the record belongs to a
    /// different tenant.
    /// </summary>
    Task<EnvironmentalHealthItemDto?> GetByIdAsync(Guid environmentalHealthId, Guid tenantId, CancellationToken cancellationToken);
}
