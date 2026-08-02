// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Permits
//  Layer:          Application
//  File:           IPermitsRepository.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for the Permits read-model. Implemented in
//                  the Infrastructure layer. Distinct from
//                  PlantSafety.Domain.Compliance.Permits.Interfaces.IPermitsRepository (the
//                  write-side aggregate repository) -- same simple name, different namespace,
//                  mirroring PlantSafety.Application.RiskManagement.Repositories.IRiskRepository /
//                  PlantSafety.Application.AccidentManagement.Repositories.IAccidentRepository's
//                  identical precedent for CQRS read/write repository separation.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Permits.Models;

namespace PlantSafety.Application.Compliance.Permits.Repositories;

/// <summary>Read-only repository for Permits data. Implemented in the Infrastructure layer.</summary>
public interface IPermitsRepository
{
    /// <summary>
    /// Returns the Permits read-model record for the given identity, scoped to the given tenant,
    /// or <see langword="null"/> if not found or if the Permits record belongs to a different tenant.
    /// </summary>
    Task<PermitsItemDto?> GetByIdAsync(Guid permitsId, Guid tenantId, CancellationToken cancellationToken);

    /// <summary>Returns Permits items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<PermitsItemDto>> GetPermitsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
