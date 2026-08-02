// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Procedures
//  Layer:          Application
//  File:           IProceduresRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for the Procedures read-model. Implemented in
//                  the Infrastructure layer. Distinct from
//                  PlantSafety.Domain.Compliance.Procedures.Interfaces.IProceduresRepository (the
//                  write-side aggregate repository) -- same simple name, different namespace,
//                  mirroring PlantSafety.Application.Compliance.Permits.Repositories.IPermitsRepository's
//                  identical precedent for CQRS read/write repository separation.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Procedures.Models;

namespace PlantSafety.Application.Compliance.Procedures.Repositories;

/// <summary>Read-only repository for Procedures data. Implemented in the Infrastructure layer.</summary>
public interface IProceduresRepository
{
    /// <summary>
    /// Returns the Procedures read-model record for the given identity, scoped to the given tenant,
    /// or <see langword="null"/> if not found or if the Procedures record belongs to a different tenant.
    /// </summary>
    Task<ProceduresItemDto?> GetByIdAsync(Guid proceduresId, Guid tenantId, CancellationToken cancellationToken);
}
