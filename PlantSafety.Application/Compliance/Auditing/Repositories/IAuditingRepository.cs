// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Auditing
//  Layer:          Application
//  File:           IAuditingRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for the Auditing read-model. Implemented in
//                  the Infrastructure layer. Distinct from
//                  PlantSafety.Domain.Compliance.Auditing.Interfaces.IAuditingRepository (the
//                  write-side aggregate repository) -- same simple name, different namespace,
//                  mirroring PlantSafety.Application.Compliance.Permits.Repositories.IPermitsRepository's
//                  identical precedent for CQRS read/write repository separation.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Auditing.Models;

namespace PlantSafety.Application.Compliance.Auditing.Repositories;

/// <summary>Read-only repository for Auditing data. Implemented in the Infrastructure layer.</summary>
public interface IAuditingRepository
{
    /// <summary>
    /// Returns the Auditing read-model record for the given identity, scoped to the given tenant,
    /// or <see langword="null"/> if not found or if the record belongs to a different tenant.
    /// </summary>
    Task<AuditingItemDto?> GetByIdAsync(Guid auditingId, Guid tenantId, CancellationToken cancellationToken);
}
