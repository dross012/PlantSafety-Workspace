// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.HumanRobotInteractionEvent
//  Layer:          Domain
//  File:           HumanRobotInteractionEventFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing HumanRobotInteractionEvent aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.HumanRobotInteractionEvent.Factories;

/// <summary>Default factory implementation for constructing <see cref="Aggregates.HumanRobotInteractionEvent"/> aggregates.</summary>
public sealed class HumanRobotInteractionEventFactory : IHumanRobotInteractionEventFactory
{
    /// <inheritdoc/>
    public Aggregates.HumanRobotInteractionEvent Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => Aggregates.HumanRobotInteractionEvent.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}
