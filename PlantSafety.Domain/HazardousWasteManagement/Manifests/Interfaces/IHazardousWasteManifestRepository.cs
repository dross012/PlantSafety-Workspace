// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.Manifests
//  Layer:          Domain
//  File:           IHazardousWasteManifestRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain repository interface for the HazardousWasteManifest aggregate root.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.Manifests.Specifications;
using PlantSafety.Domain.HazardousWasteManagement.Manifests.ValueObjects.Ids;

namespace PlantSafety.Domain.HazardousWasteManagement.Manifests.Interfaces;

/// <summary>Domain repository interface for the HazardousWasteManifest aggregate root.</summary>
public interface IHazardousWasteManifestRepository
{
    /// <summary>Retrieves a HazardousWasteManifest aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.HazardousWasteManagement.Manifests.Aggregates.HazardousWasteManifest?> GetByIdAsync(HazardousWasteManifestId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all HazardousWasteManifest aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.HazardousWasteManagement.Manifests.Aggregates.HazardousWasteManifest>> ListAsync(HazardousWasteManifestSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new HazardousWasteManifest aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.HazardousWasteManagement.Manifests.Aggregates.HazardousWasteManifest aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing HazardousWasteManifest aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.HazardousWasteManagement.Manifests.Aggregates.HazardousWasteManifest aggregate, CancellationToken cancellationToken = default);
}
