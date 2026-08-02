// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Culture
//  Layer:          Application
//  File:           ICultureReadRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for the Culture read-model. Implemented in the
//                  Infrastructure layer. Distinct from
//                  PlantSafety.Application.Common.Interfaces.Persistence.ICultureRepository (the
//                  write-side aggregate repository) -- same kind of CQRS read/write repository
//                  separation as
//                  PlantSafety.Application.Compliance.Permits.Repositories.IPermitsRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Analytics.Culture.Models;

namespace PlantSafety.Application.Analytics.Culture.Repositories;

/// <summary>Read-only repository for Culture data. Implemented in the Infrastructure layer.</summary>
public interface ICultureReadRepository
{
    /// <summary>
    /// Returns the Culture read-model record for the given identity, scoped to the given tenant,
    /// or <see langword="null"/> if not found or if the Culture record belongs to a different tenant.
    /// </summary>
    Task<CultureItemDto?> GetByIdAsync(Guid cultureId, Guid tenantId, CancellationToken cancellationToken);
}
