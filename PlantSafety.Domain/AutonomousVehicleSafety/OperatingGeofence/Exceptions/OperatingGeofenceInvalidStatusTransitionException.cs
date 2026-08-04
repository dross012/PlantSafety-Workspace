// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.OperatingGeofence
//  Layer:          Domain
//  File:           OperatingGeofenceInvalidStatusTransitionException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when an OperatingGeofence lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.OperatingGeofence.Exceptions;

/// <summary>Thrown when an <see cref="Aggregates.OperatingGeofence"/> lifecycle transition is not permitted.</summary>
public sealed class OperatingGeofenceInvalidStatusTransitionException : OperatingGeofenceDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public OperatingGeofenceInvalidStatusTransitionException(
        OperatingGeofenceId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'OperatingGeofence' {id} from '{from}' to '{to}'.",
            "OperatingGeofence.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
