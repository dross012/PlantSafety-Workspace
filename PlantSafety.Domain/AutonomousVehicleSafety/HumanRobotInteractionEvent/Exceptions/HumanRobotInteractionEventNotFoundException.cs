// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.HumanRobotInteractionEvent
//  Layer:          Domain
//  File:           HumanRobotInteractionEventNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a HumanRobotInteractionEvent aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.HumanRobotInteractionEvent.Exceptions;

/// <summary>Thrown when a <see cref="Aggregates.HumanRobotInteractionEvent"/> aggregate lookup finds no match.</summary>
public sealed class HumanRobotInteractionEventNotFoundException : HumanRobotInteractionEventDomainException
{
    /// <summary>Creates a not-found exception for the given HumanRobotInteractionEvent identity.</summary>
    public HumanRobotInteractionEventNotFoundException(HumanRobotInteractionEventId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"HumanRobotInteractionEvent with id '{id}' was not found.",
            "HumanRobotInteractionEvent.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
