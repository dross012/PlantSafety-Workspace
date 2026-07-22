// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.SiteOrientation
//  Layer:          Domain
//  File:           ISiteOrientationRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the SiteOrientation aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.SiteOrientation.Interfaces;

/// <summary>Domain repository interface for the <see cref="SiteOrientation"/> aggregate root.</summary>
public interface ISiteOrientationRepository
{
    /// <summary>Retrieves a SiteOrientation aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.ContractorSafety.SiteOrientation.Aggregates.SiteOrientation?> GetByIdAsync(SiteOrientationId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all SiteOrientation aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.ContractorSafety.SiteOrientation.Aggregates.SiteOrientation>> ListAsync(SiteOrientationSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new SiteOrientation aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.ContractorSafety.SiteOrientation.Aggregates.SiteOrientation aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing SiteOrientation aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.ContractorSafety.SiteOrientation.Aggregates.SiteOrientation aggregate, CancellationToken cancellationToken = default);
}