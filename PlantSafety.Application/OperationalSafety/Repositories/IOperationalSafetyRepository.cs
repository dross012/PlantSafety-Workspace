// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         OperationalSafety
//  Layer:          Application
//  File:           IOperationalSafetyRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for the OperationalSafety read-model.
//                  Implemented in the Infrastructure layer. Distinct from
//                  PlantSafety.Domain.OperationalSafety.Repositories.IOperationalSafetyRepository
//                  (the write-side aggregate repository) -- same simple name, different
//                  namespace, mirroring
//                  PlantSafety.Application.Compliance.Auditing.Repositories.IAuditingRepository's
//                  identical precedent for CQRS read/write repository separation. Also distinct
//                  from the pre-existing, unrelated, LIVE OperationalSafety.Alerts/Checklists/
//                  PPEManagement/RiskManagement/Schedules Infrastructure interfaces.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.OperationalSafety.Models;

namespace PlantSafety.Application.OperationalSafety.Repositories;

/// <summary>Read-only repository for OperationalSafety data. Implemented in the Infrastructure layer.</summary>
public interface IOperationalSafetyRepository
{
    /// <summary>
    /// Returns the OperationalSafety read-model record for the given identity, scoped to the
    /// given tenant, or <see langword="null"/> if not found or if the record belongs to a
    /// different tenant.
    /// </summary>
    Task<OperationalSafetyItemDto?> GetByIdAsync(Guid operationalSafetyId, Guid tenantId, CancellationToken cancellationToken);
}
