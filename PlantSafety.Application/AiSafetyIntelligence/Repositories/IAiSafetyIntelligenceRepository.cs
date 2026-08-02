// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AiSafetyIntelligence
//  Layer:          Application
//  File:           IAiSafetyIntelligenceRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for the AiSafetyIntelligence read-model.
//                  Implemented in the Infrastructure layer. Distinct from
//                  PlantSafety.Domain.AiSafetyIntelligence.Repositories.IAiSafetyIntelligenceRepository
//                  (the write-side aggregate repository) -- same simple name, different namespace.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.AiSafetyIntelligence.Models;

namespace PlantSafety.Application.AiSafetyIntelligence.Repositories;

/// <summary>Read-only repository for AiSafetyIntelligence data. Implemented in the Infrastructure layer.</summary>
public interface IAiSafetyIntelligenceRepository
{
    /// <summary>
    /// Returns the AiSafetyIntelligence read-model record for the given identity, scoped to the
    /// given tenant, or <see langword="null"/> if not found or if the record belongs to a
    /// different tenant.
    /// </summary>
    Task<AiSafetyIntelligenceItemDto?> GetByIdAsync(Guid aiSafetyIntelligenceId, Guid tenantId, CancellationToken cancellationToken);
}
