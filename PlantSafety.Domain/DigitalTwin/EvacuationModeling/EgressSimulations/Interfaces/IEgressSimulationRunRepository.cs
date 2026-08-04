// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EgressSimulations
//  Layer:          Domain
//  File:           IEgressSimulationRunRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the EgressSimulationRun aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.Interfaces;

/// <summary>Domain repository interface for the <see cref="Aggregates.EgressSimulationRun"/> aggregate root.</summary>
public interface IEgressSimulationRunRepository
{
    /// <summary>Retrieves an EgressSimulationRun aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.Aggregates.EgressSimulationRun?> GetByIdAsync(EgressSimulationRunId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all EgressSimulationRun aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.Aggregates.EgressSimulationRun>> ListAsync(EgressSimulationRunSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new EgressSimulationRun aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.Aggregates.EgressSimulationRun aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing EgressSimulationRun aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.Aggregates.EgressSimulationRun aggregate, CancellationToken cancellationToken = default);
}
