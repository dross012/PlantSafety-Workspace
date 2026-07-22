// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.EmergencyResponsePlanning
//  Layer:          Domain
//  File:           IEmergencyResponsePlanningRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the EmergencyResponsePlanning aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.EmergencyResponsePlanning.Interfaces;

/// <summary>Domain repository interface for the <see cref="EmergencyResponsePlanning"/> aggregate root.</summary>
public interface IEmergencyResponsePlanningRepository
{
    /// <summary>Retrieves a EmergencyResponsePlanning aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.SafetyProcedures.EmergencyResponsePlanning.Aggregates.EmergencyResponsePlanning?> GetByIdAsync(EmergencyResponsePlanningId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all EmergencyResponsePlanning aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.SafetyProcedures.EmergencyResponsePlanning.Aggregates.EmergencyResponsePlanning>> ListAsync(EmergencyResponsePlanningSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new EmergencyResponsePlanning aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.SafetyProcedures.EmergencyResponsePlanning.Aggregates.EmergencyResponsePlanning aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing EmergencyResponsePlanning aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.SafetyProcedures.EmergencyResponsePlanning.Aggregates.EmergencyResponsePlanning aggregate, CancellationToken cancellationToken = default);
}