// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.AutonomousVehicleIncident
//  Layer:          Domain
//  File:           AutonomousVehicleIncidentInvalidStatusTransitionException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when an AutonomousVehicleIncident lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleIncident.Exceptions;

/// <summary>Thrown when a <see cref="Aggregates.AutonomousVehicleIncident"/> lifecycle transition is not permitted.</summary>
public sealed class AutonomousVehicleIncidentInvalidStatusTransitionException : AutonomousVehicleIncidentDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public AutonomousVehicleIncidentInvalidStatusTransitionException(
        AutonomousVehicleIncidentId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'AutonomousVehicleIncident' {id} from '{from}' to '{to}'.",
            "AutonomousVehicleIncident.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
