// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.AutonomousVehicleIncident
//  Layer:          Domain
//  File:           AutonomousVehicleIncidentNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when an AutonomousVehicleIncident aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleIncident.Exceptions;

/// <summary>Thrown when a <see cref="Aggregates.AutonomousVehicleIncident"/> aggregate lookup finds no match.</summary>
public sealed class AutonomousVehicleIncidentNotFoundException : AutonomousVehicleIncidentDomainException
{
    /// <summary>Creates a not-found exception for the given AutonomousVehicleIncident identity.</summary>
    public AutonomousVehicleIncidentNotFoundException(AutonomousVehicleIncidentId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"AutonomousVehicleIncident with id '{id}' was not found.",
            "AutonomousVehicleIncident.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
