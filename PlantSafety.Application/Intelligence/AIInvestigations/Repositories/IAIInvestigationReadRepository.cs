// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.AIInvestigations
//  Layer:          Application
//  File:           IAIInvestigationReadRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for the AIInvestigation aggregate's by-id
//                  detail query. Named "IAIInvestigationReadRepository" (not
//                  "IAIInvestigationRepository") to avoid colliding with the Domain-layer
//                  write-side PlantSafety.Domain.Intelligence.AIInvestigations.Interfaces
//                  .IAIInvestigationRepository. Implemented in the Infrastructure layer.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Intelligence.AIInvestigations.Models;

namespace PlantSafety.Application.Intelligence.AIInvestigations.Repositories;

/// <summary>Read-only repository for the AIInvestigation aggregate's by-id detail query.</summary>
public interface IAIInvestigationReadRepository
{
    /// <summary>
    /// Returns the full AIInvestigation read-model record for the given identity, scoped to the
    /// given tenant, or <see langword="null"/> if not found or if it belongs to a different
    /// tenant.
    /// </summary>
    Task<AIInvestigationDetailDto?> GetByIdAsync(Guid aiInvestigationId, Guid tenantId, CancellationToken cancellationToken);
}
