// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.HumanRobotInteractionEvent
//  Layer:          Domain
//  File:           HumanRobotInteractionEventInvalidStatusTransitionException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a HumanRobotInteractionEvent lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.HumanRobotInteractionEvent.Exceptions;

/// <summary>Thrown when a <see cref="Aggregates.HumanRobotInteractionEvent"/> lifecycle transition is not permitted.</summary>
public sealed class HumanRobotInteractionEventInvalidStatusTransitionException : HumanRobotInteractionEventDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public HumanRobotInteractionEventInvalidStatusTransitionException(
        HumanRobotInteractionEventId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'HumanRobotInteractionEvent' {id} from '{from}' to '{to}'.",
            "HumanRobotInteractionEvent.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
