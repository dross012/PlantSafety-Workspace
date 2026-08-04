// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.OperatingGeofence
//  Layer:          Domain
//  File:           OperatingGeofenceNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when an OperatingGeofence aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.OperatingGeofence.Exceptions;

/// <summary>Thrown when an <see cref="Aggregates.OperatingGeofence"/> aggregate lookup finds no match.</summary>
public sealed class OperatingGeofenceNotFoundException : OperatingGeofenceDomainException
{
    /// <summary>Creates a not-found exception for the given OperatingGeofence identity.</summary>
    public OperatingGeofenceNotFoundException(OperatingGeofenceId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"OperatingGeofence with id '{id}' was not found.",
            "OperatingGeofence.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
