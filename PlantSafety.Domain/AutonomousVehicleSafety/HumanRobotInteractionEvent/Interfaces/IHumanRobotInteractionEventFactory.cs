// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.HumanRobotInteractionEvent
//  Layer:          Domain
//  File:           IHumanRobotInteractionEventFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing HumanRobotInteractionEvent aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.HumanRobotInteractionEvent.Interfaces;

/// <summary>Factory interface for constructing <see cref="Aggregates.HumanRobotInteractionEvent"/> aggregates.</summary>
public interface IHumanRobotInteractionEventFactory
{
    /// <summary>Creates a new HumanRobotInteractionEvent aggregate with the supplied parameters.</summary>
    Aggregates.HumanRobotInteractionEvent Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}
