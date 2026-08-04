// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationBottlenecks
//  Layer:          Domain
//  File:           IEvacuationBottleneckRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the EvacuationBottleneck aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Interfaces;

/// <summary>Domain repository interface for the <see cref="Aggregates.EvacuationBottleneck"/> aggregate root.</summary>
public interface IEvacuationBottleneckRepository
{
    /// <summary>Retrieves an EvacuationBottleneck aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Aggregates.EvacuationBottleneck?> GetByIdAsync(EvacuationBottleneckId id, CancellationToken cancellationToken = default);

    /// <summary>Retrieves the currently open (non-mitigated) bottleneck for a route, if any.</summary>
    Task<PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Aggregates.EvacuationBottleneck?> GetOpenForRouteAsync(EvacuationRouteId routeId, CancellationToken cancellationToken = default);

    /// <summary>Returns all EvacuationBottleneck aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Aggregates.EvacuationBottleneck>> ListAsync(EvacuationBottleneckSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new EvacuationBottleneck aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Aggregates.EvacuationBottleneck aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing EvacuationBottleneck aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Aggregates.EvacuationBottleneck aggregate, CancellationToken cancellationToken = default);
}
