// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationRoutes
//  Layer:          Domain
//  File:           IEvacuationRouteRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the EvacuationRoute aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Interfaces;

/// <summary>Domain repository interface for the <see cref="EvacuationRoute"/> aggregate root.</summary>
public interface IEvacuationRouteRepository
{
    /// <summary>Retrieves an EvacuationRoute aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Aggregates.EvacuationRoute?> GetByIdAsync(EvacuationRouteId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all EvacuationRoute aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Aggregates.EvacuationRoute>> ListAsync(EvacuationRouteSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new EvacuationRoute aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Aggregates.EvacuationRoute aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing EvacuationRoute aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Aggregates.EvacuationRoute aggregate, CancellationToken cancellationToken = default);
}
