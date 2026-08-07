// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Packaging
//  Layer:          Application
//  File:           IPackagingSpecificationRepository.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for the PackagingSpecification read-model.
//                  Implemented in the Infrastructure layer. Distinct from
//                  PlantSafety.Domain.Compliance.PHMSA.Packaging.Interfaces.
//                  IPackagingSpecificationRepository (the write-side aggregate repository) -- same
//                  simple name, different namespace, mirroring
//                  PlantSafety.Application.Compliance.Permits.Repositories.IPermitsRepository's
//                  identical precedent for CQRS read/write repository separation.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.PHMSA.Packaging.Models;

namespace PlantSafety.Application.Compliance.PHMSA.Packaging.Repositories;

/// <summary>Read-only repository for PackagingSpecification data. Implemented in the Infrastructure layer.</summary>
public interface IPackagingSpecificationRepository
{
    /// <summary>
    /// Returns the PackagingSpecification read-model record for the given identity, scoped to
    /// the given tenant, or <see langword="null"/> if not found or if the record belongs to a
    /// different tenant.
    /// </summary>
    Task<PackagingSpecificationDto?> GetByIdAsync(Guid packagingSpecificationId, Guid tenantId, CancellationToken cancellationToken);

    /// <summary>Returns PackagingSpecification items for the tenant/site, optionally filtered by DOT spec code.</summary>
    Task<IReadOnlyList<PackagingSpecificationDto>> ListAsync(
        Guid tenantId,
        Guid? siteId,
        string? dotSpec,
        CancellationToken cancellationToken);
}
