// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Policy
//  Layer:          Application
//  File:           IPolicyRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for the Policy read-model. Implemented in the
//                  Infrastructure layer. Distinct from
//                  PlantSafety.Domain.Compliance.Policy.Interfaces.IPolicyRepository (the write-side
//                  aggregate repository) -- same simple name, different namespace, mirroring
//                  PlantSafety.Application.Compliance.Permits.Repositories.IPermitsRepository's
//                  identical precedent for CQRS read/write repository separation.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Policy.Models;

namespace PlantSafety.Application.Compliance.Policy.Repositories;

/// <summary>Read-only repository for Policy data. Implemented in the Infrastructure layer.</summary>
public interface IPolicyRepository
{
    /// <summary>
    /// Returns the Policy read-model record for the given identity, scoped to the given tenant, or
    /// <see langword="null"/> if not found or if the Policy record belongs to a different tenant.
    /// </summary>
    Task<PolicyItemDto?> GetByIdAsync(Guid policyId, Guid tenantId, CancellationToken cancellationToken);
}
