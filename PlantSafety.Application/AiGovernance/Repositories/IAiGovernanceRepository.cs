// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AiGovernance
//  Layer:          Application
//  File:           IAiGovernanceRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for the AiGovernance read-model. Implemented
//                  in the Infrastructure layer. Distinct from
//                  PlantSafety.Domain.AiGovernance.Repositories.IAiGovernanceRepository (the
//                  write-side aggregate repository) -- same simple name, different namespace,
//                  mirroring PlantSafety.Application.Compliance.Auditing.Repositories.IAuditingRepository's
//                  identical precedent for CQRS read/write repository separation.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.AiGovernance.Models;

namespace PlantSafety.Application.AiGovernance.Repositories;

/// <summary>Read-only repository for AiGovernance data. Implemented in the Infrastructure layer.</summary>
public interface IAiGovernanceRepository
{
    /// <summary>
    /// Returns the AiGovernance read-model record for the given identity, scoped to the given
    /// tenant, or <see langword="null"/> if not found or if the record belongs to a different
    /// tenant.
    /// </summary>
    Task<AiGovernanceItemDto?> GetByIdAsync(Guid aiGovernanceId, Guid tenantId, CancellationToken cancellationToken);
}
