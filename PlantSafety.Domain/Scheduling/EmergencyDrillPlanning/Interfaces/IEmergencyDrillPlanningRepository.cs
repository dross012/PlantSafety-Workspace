// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.EmergencyDrillPlanning
//  Layer:          Domain
//  File:           IEmergencyDrillPlanningRepository.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the EmergencyDrillPlanning aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Interfaces;

/// <summary>Domain repository interface for the <see cref="EmergencyDrillPlanning"/> aggregate root.</summary>
public interface IEmergencyDrillPlanningRepository
{
    /// <summary>Retrieves a EmergencyDrillPlanning aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Aggregates.EmergencyDrillPlanning?> GetByIdAsync(EmergencyDrillPlanningId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all EmergencyDrillPlanning aggregates matching the given specification.</summary>
    Task<IReadOnlyList<PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Aggregates.EmergencyDrillPlanning>> ListAsync(EmergencyDrillPlanningSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new EmergencyDrillPlanning aggregate.</summary>
    Task AddAsync(PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Aggregates.EmergencyDrillPlanning aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing EmergencyDrillPlanning aggregate.</summary>
    Task UpdateAsync(PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Aggregates.EmergencyDrillPlanning aggregate, CancellationToken cancellationToken = default);
}