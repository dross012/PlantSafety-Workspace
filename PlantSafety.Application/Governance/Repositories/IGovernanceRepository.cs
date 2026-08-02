// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Governance
//  Layer:          Application
//  File:           IGovernanceRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for the Governance read-model. Implemented
//                  in the Infrastructure layer. Distinct from
//                  PlantSafety.Domain.Governance.Repositories.IGovernanceRepository (the
//                  write-side aggregate repository) -- same simple name, different namespace,
//                  mirroring PlantSafety.Application.Compliance.Auditing.Repositories.IAuditingRepository's
//                  identical precedent for CQRS read/write repository separation.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Governance.Models;

namespace PlantSafety.Application.Governance.Repositories;

/// <summary>Read-only repository for Governance data. Implemented in the Infrastructure layer.</summary>
public interface IGovernanceRepository
{
    /// <summary>
    /// Returns the Governance read-model record for the given identity, scoped to the given
    /// tenant, or <see langword="null"/> if not found or if the record belongs to a different
    /// tenant.
    /// </summary>
    Task<GovernanceItemDto?> GetByIdAsync(Guid governanceId, Guid tenantId, CancellationToken cancellationToken);
}
