// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.HumanRobotInteractionEvent
//  Layer:          Domain
//  File:           IHumanRobotInteractionEventRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain repository interface for the HumanRobotInteractionEvent aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.HumanRobotInteractionEvent.Interfaces;

/// <summary>Domain repository interface for the <see cref="Aggregates.HumanRobotInteractionEvent"/> aggregate root.</summary>
public interface IHumanRobotInteractionEventRepository
{
    /// <summary>Retrieves a HumanRobotInteractionEvent aggregate by its identity, or <see langword="null"/> if not found.</summary>
    Task<Aggregates.HumanRobotInteractionEvent?> GetByIdAsync(HumanRobotInteractionEventId id, CancellationToken cancellationToken = default);

    /// <summary>Returns all HumanRobotInteractionEvent aggregates matching the given specification.</summary>
    Task<IReadOnlyList<Aggregates.HumanRobotInteractionEvent>> ListAsync(HumanRobotInteractionEventSpecification specification, CancellationToken cancellationToken = default);

    /// <summary>Persists a new HumanRobotInteractionEvent aggregate.</summary>
    Task AddAsync(Aggregates.HumanRobotInteractionEvent aggregate, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing HumanRobotInteractionEvent aggregate.</summary>
    Task UpdateAsync(Aggregates.HumanRobotInteractionEvent aggregate, CancellationToken cancellationToken = default);
}
