// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.WasteStreams
//  Layer:          Domain
//  File:           IWasteStreamRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain repository interface for the WasteStream aggregate root.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.WasteStreams.Specifications;
using PlantSafety.Domain.HazardousWasteManagement.WasteStreams.ValueObjects.Ids;

namespace PlantSafety.Domain.HazardousWasteManagement.WasteStreams.Interfaces;

/// <summary>Domain repository interface for the WasteStream aggregate root.</summary>
public interface IWasteStreamRepository
{
    /// <summary>Retrieves a WasteStream aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.HazardousWasteManagement.WasteStreams.Aggregates.WasteStream?> GetByIdAsync(WasteStreamId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all WasteStream aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.HazardousWasteManagement.WasteStreams.Aggregates.WasteStream>> ListAsync(WasteStreamSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new WasteStream aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.HazardousWasteManagement.WasteStreams.Aggregates.WasteStream aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing WasteStream aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.HazardousWasteManagement.WasteStreams.Aggregates.WasteStream aggregate, CancellationToken cancellationToken = default);
}
