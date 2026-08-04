// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.WasteContainers
//  Layer:          Domain
//  File:           IWasteContainerRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain repository interface for the WasteContainer aggregate root.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.WasteContainers.Specifications;
using PlantSafety.Domain.HazardousWasteManagement.WasteContainers.ValueObjects.Ids;

namespace PlantSafety.Domain.HazardousWasteManagement.WasteContainers.Interfaces;

/// <summary>Domain repository interface for the WasteContainer aggregate root.</summary>
public interface IWasteContainerRepository
{
    /// <summary>Retrieves a WasteContainer aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.HazardousWasteManagement.WasteContainers.Aggregates.WasteContainer?> GetByIdAsync(WasteContainerId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all WasteContainer aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.HazardousWasteManagement.WasteContainers.Aggregates.WasteContainer>> ListAsync(WasteContainerSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new WasteContainer aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.HazardousWasteManagement.WasteContainers.Aggregates.WasteContainer aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing WasteContainer aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.HazardousWasteManagement.WasteContainers.Aggregates.WasteContainer aggregate, CancellationToken cancellationToken = default);
}
